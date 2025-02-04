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

export class AcmeInbox extends Construct {
  public readonly table: TableV2;
  public readonly deadLetterQueue: Queue;
  public readonly queue: Queue;
  public readonly processorFunction: Function;

  constructor(scope: Construct, id: string) {
    super(scope, id);

    this.table = new TableV2(this, `${this.node.id}-table`, {
      tableName: `${this.node.id}-table`,
      partitionKey: { name: "id", type: AttributeType.STRING },
      dynamoStream: StreamViewType.NEW_AND_OLD_IMAGES,
      removalPolicy: RemovalPolicy.DESTROY,
      timeToLiveAttribute: "ttl",
    });

    this.deadLetterQueue = new Queue(this, `${this.node.id}-dlq`, {
      queueName: `${this.node.id}-inbox-dlq`,
    });

    this.queue = new Queue(this, `${this.node.id}-queue`, {
      queueName: `${this.node.id}-inbox-queue`,
      deadLetterQueue: {
        queue: this.deadLetterQueue,
        maxReceiveCount: 3,
      },
    });

    this.processorFunction = new Function(
      this,
      `${this.node.id}-processor-function`,
      {
        functionName: `${this.node.id}-processor-function`,
        code: zipAssetResolver("InboxProcessor"),
        handler: createHandler("Acme", "InboxProcessor"),
        runtime: Runtime.DOTNET_8,
      }
    );

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
