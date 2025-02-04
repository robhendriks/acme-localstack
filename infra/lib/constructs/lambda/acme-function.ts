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

export interface AcmeFunctionProps {
  projectName: string;
  handler?: string;
  runtime?: Runtime;
}

export class AcmeFunction extends Construct {
  public readonly function: Function;

  private _httpApi?: IHttpApi;
  private _httpApiIntegration?: HttpLambdaIntegration;

  constructor(scope: Construct, id: string, props: AcmeFunctionProps) {
    super(scope, id);

    this.function = new Function(this, `${this.node.id}-function`, {
      functionName: `${this.node.id}-function`,
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
      `${this.node.id}-integration`,
      this.function
    ));
  }

  public addRoute(id: string, path: string, method: HttpMethod): AcmeFunction {
    new HttpRoute(this, `${this.node.id}-route-${id}}`, {
      httpApi: this.getHttpApi(),
      routeKey: HttpRouteKey.with(path, method),
      integration: this.getHttpIntegration(),
    });

    return this;
  }

  public addOutbox(topic: AcmeTopic): AcmeFunction {
    topic.outboxTable.grantFullAccess(this.function);

    new StringParameter(this, `${this.node.id}-param-outbox-table-name`, {
      parameterName: `/${this.node.id}/Outbox/TableName`,
      stringValue: topic.outboxTable.tableName,
    });

    return this;
  }

  public addEntityDb(db: AcmeEntityDb): AcmeFunction {
    new StringParameter(
      this,
      `${this.node.id}-param-${kebabCase(db.entityName)}-table-name`,
      {
        parameterName: `/${this.node.id}/${pascalCase(
          db.entityName
        )}/TableName`,
        stringValue: db.table.tableName,
      }
    );

    return this;
  }
}
