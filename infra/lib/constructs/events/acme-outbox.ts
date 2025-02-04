import { RemovalPolicy } from "aws-cdk-lib";
import {
  TableV2,
  AttributeType,
  StreamViewType,
} from "aws-cdk-lib/aws-dynamodb";
import { Role, ServicePrincipal } from "aws-cdk-lib/aws-iam";
import { Runtime, Function } from "aws-cdk-lib/aws-lambda";
import { SqsEventSource } from "aws-cdk-lib/aws-lambda-event-sources";
import { CfnPipe } from "aws-cdk-lib/aws-pipes";
import { Queue } from "aws-cdk-lib/aws-sqs";
import { Construct } from "constructs";
import { zipAssetResolver, createHandler } from "../../util/lambda";
import { generateName } from "../../util/construct";

export interface AcmeOutboxProps {
  topicName: string;
}

export class AcmeOutbox extends Construct {
  public readonly table: TableV2;
  public readonly deadLetterQueue: Queue;
  public readonly queue: Queue;
  public readonly processorFunction: Function;
  public readonly pipeRole: Role;
  public readonly pipe: CfnPipe;

  constructor(scope: Construct, id: string, props: AcmeOutboxProps) {
    super(scope, id);

    this.table = new TableV2(this, "table", {
      tableName: generateName(this.node, "table"),
      partitionKey: { name: "id", type: AttributeType.STRING },
      dynamoStream: StreamViewType.NEW_AND_OLD_IMAGES,
      removalPolicy: RemovalPolicy.DESTROY,
      timeToLiveAttribute: "ttl",
    });

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

    this.processorFunction = new Function(this, "function", {
      functionName: generateName(this.node, "function"),
      code: zipAssetResolver("OutboxProcessor"),
      handler: createHandler("Acme", "OutboxProcessor"),
      runtime: Runtime.DOTNET_8,
    });

    // Configure outbox table in processor
    this.processorFunction.addEnvironment(
      "OUTBOX_TABLE_NAME",
      this.table.tableName
    );

    // Subscribe processor function to outbox queue
    this.queue.grantConsumeMessages(this.processorFunction);
    this.processorFunction.addEventSource(
      new SqsEventSource(this.queue, {
        reportBatchItemFailures: true,
      })
    );

    // Pipe DynamoDB INSERT events into outbox SQS queue
    this.pipeRole = new Role(this, "role-pipe", {
      roleName: generateName(this.node, "role-pipe"),
      assumedBy: new ServicePrincipal("pipes.amazonaws.com"),
    });

    this.pipe = new CfnPipe(this, "pipe", {
      name: generateName(this.node, "pipe"),
      roleArn: this.pipeRole.roleArn,
      source: this.table.tableStreamArn!,
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
                  NewImage: { topic: { S: [props.topicName] } },
                },
              }),
            },
          ],
        },
      },
      target: this.queue.queueArn,
    });
  }
}
