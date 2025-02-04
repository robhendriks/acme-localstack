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
import { StringParameter } from "aws-cdk-lib/aws-ssm";

export class AcmeInbox extends Construct {
  public readonly table: TableV2;
  public readonly deadLetterQueue: Queue;
  public readonly queue: Queue;
  public readonly function: Function;

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

    this.function = new Function(this, "function", {
      functionName: generateName(this.node, "function"),
      code: zipAssetResolver("InboxProcessor"),
      handler: createHandler("Acme", "InboxProcessor"),
      runtime: Runtime.DOTNET_8,
    });

    this.function.addEventSource(
      new SqsEventSource(this.queue, {
        reportBatchItemFailures: true,
      })
    );

    this.function.addEnvironment("INBOX_TABLE_NAME", this.table.tableName);

    // new StringParameter(this, "param-inbox-table-name", {
    //   parameterName: `/${this.function.node.id}/Inbox/TableName`,
    //   stringValue: this.table.tableName,
    // });

    this.table.grantFullAccess(this.function);
  }
}
