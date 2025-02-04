import { RemovalPolicy } from "aws-cdk-lib";
import {
  TableV2,
  AttributeType,
  StreamViewType,
} from "aws-cdk-lib/aws-dynamodb";
import { Runtime, Function } from "aws-cdk-lib/aws-lambda";
import { SqsSubscription } from "aws-cdk-lib/aws-sns-subscriptions";
import { Queue } from "aws-cdk-lib/aws-sqs";
import { Construct } from "constructs";
import { zipAssetResolver, createHandler } from "../../util/lambda";
import { AcmeTopic } from "./acme-topic";
import { SqsEventSource } from "aws-cdk-lib/aws-lambda-event-sources";
import { generateName } from "../../util/construct";

export class AcmeInbox extends Construct {
  public readonly table: TableV2;
  public readonly deadLetterQueue: Queue;
  public readonly queue: Queue;
  public readonly processorFunction: Function;

  constructor(scope: Construct, id: string) {
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

    this.processorFunction = new Function(this, "function-processor", {
      functionName: generateName(this.node, "function-processor"),
      code: zipAssetResolver("InboxProcessor"),
      handler: createHandler("Acme", "InboxProcessor"),
      runtime: Runtime.DOTNET_8,
    });

    this.processorFunction.addEventSource(
      new SqsEventSource(this.queue, {
        reportBatchItemFailures: true,
      })
    );

    this.processorFunction.addEnvironment(
      "INBOX_TABLE_NAME",
      this.table.tableName
    );

    this.table.grantFullAccess(this.processorFunction);
  }
}
