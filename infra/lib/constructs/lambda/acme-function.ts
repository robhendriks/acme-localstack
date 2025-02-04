import {
  HttpMethod,
  HttpRoute,
  HttpRouteKey,
  IHttpApi,
} from "aws-cdk-lib/aws-apigatewayv2";
import { HttpLambdaIntegration } from "aws-cdk-lib/aws-apigatewayv2-integrations";
import { Function, Runtime } from "aws-cdk-lib/aws-lambda";
import { Construct } from "constructs";
import { importHttpApi } from "../../util/http-api";
import { StringParameter } from "aws-cdk-lib/aws-ssm";
import { AcmeEntityDb } from "../storage/acme-entity-db";
import { pascalCase } from "pascal-case";
import kebabCase from "kebab-case";
import { createHandler, zipAssetResolver } from "../../util/lambda";
import { AcmeTopic } from "../events/acme-topic";
import { Queue } from "aws-cdk-lib/aws-sqs";
import { SqsEventSource } from "aws-cdk-lib/aws-lambda-event-sources";
import { Role, ServicePrincipal } from "aws-cdk-lib/aws-iam";
import { CfnPipe } from "aws-cdk-lib/aws-pipes";
import { generateName } from "../../util/construct";

export interface AcmeFunctionProps {
  projectName: string;
  handler?: string;
  runtime?: Runtime;
}

export class AcmeFunction extends Construct {
  public readonly function: Function;

  public deadLetterQueue?: Queue;
  public queue?: Queue;

  private _httpApi?: IHttpApi;
  private _httpApiIntegration?: HttpLambdaIntegration;

  constructor(scope: Construct, id: string, props: AcmeFunctionProps) {
    super(scope, id);

    this.function = new Function(this, "function", {
      functionName: generateName(this.node, "function"),
      code: zipAssetResolver(props.projectName),
      handler: props.handler ?? createHandler("Acme", props.projectName),
      runtime: props.runtime ?? Runtime.DOTNET_8,
    });

    this.function.addEnvironment("ACME_APPLICATION", this.node.id);
  }

  private getHttpApi(): IHttpApi {
    return (this._httpApi ??= importHttpApi(this));
  }

  private getHttpIntegration(): HttpLambdaIntegration {
    return (this._httpApiIntegration ??= new HttpLambdaIntegration(
      "integration",
      this.function
    ));
  }

  public addQueue(): AcmeFunction {
    if (this.queue) {
      console.warn("Queue already exists, skipping creation.");
      return this;
    }

    this.deadLetterQueue = new Queue(this, "dlq", {
      queueName: generateName(this.node, "dlq"),
    });

    this.queue = new Queue(this, "queue", {
      queueName: generateName(this.node, "queue"),
      deadLetterQueue: {
        queue: this.deadLetterQueue,
        maxReceiveCount: 3,
      },
    });

    this.queue.grantConsumeMessages(this.function);

    this.function.addEventSource(
      new SqsEventSource(this.queue, {
        reportBatchItemFailures: true,
      })
    );

    return this;
  }

  public addRoute(id: string, path: string, method: HttpMethod): AcmeFunction {
    new HttpRoute(this, `route-${id}`, {
      httpApi: this.getHttpApi(),
      routeKey: HttpRouteKey.with(path, method),
      integration: this.getHttpIntegration(),
    });

    return this;
  }

  public addOutbox(topic: AcmeTopic): AcmeFunction {
    topic.outbox.table.grantFullAccess(this.function);

    new StringParameter(this, "param-outbox-table-name", {
      parameterName: `/${this.node.id}/Outbox/TableName`,
      stringValue: topic.outbox.table.tableName,
    });

    return this;
  }

  public addInbox(topic: AcmeTopic): AcmeFunction {
    if (!this.queue) {
      console.warn("Queue not configured, skipping inbox link.");
      return this;
    }

    new StringParameter(this, "param-inbox-table-name", {
      parameterName: `/${this.node.id}/Inbox/TableName`,
      stringValue: topic.inbox.table.tableName,
    });

    const pipeRole = new Role(this, "role-pipe", {
      roleName: generateName(this.node, "role-pipe"),
      assumedBy: new ServicePrincipal("pipes.amazonaws.com"),
    });

    new CfnPipe(this, "pipe", {
      name: generateName(this.node, "pipe"),
      roleArn: pipeRole.roleArn,
      source: topic.inbox.table.tableStreamArn!,
      sourceParameters: {
        dynamoDbStreamParameters: {
          startingPosition: "LATEST",
          batchSize: 10,
        },
        filterCriteria: {
          filters: [
            {
              pattern: JSON.stringify({
                eventName: ["INSERT"],
                dynamodb: {
                  NewImage: { topic: { S: [topic.topicName] } },
                },
              }),
            },
          ],
        },
      },
      target: this.queue.queueArn,
    });

    return this;
  }

  public addEntityDb(db: AcmeEntityDb): AcmeFunction {
    new StringParameter(this, `param-${kebabCase(db.entityName)}-table-name`, {
      parameterName: `/${this.node.id}/${pascalCase(db.entityName)}/TableName`,
      stringValue: db.table.tableName,
    });

    return this;
  }
}
