import { Queue } from "aws-cdk-lib/aws-sqs";
import { Function, Runtime } from "aws-cdk-lib/aws-lambda";
import { Construct } from "constructs";
import { Role, ServicePrincipal } from "aws-cdk-lib/aws-iam";
import { CfnPipe } from "aws-cdk-lib/aws-pipes";
import { createHandler, zipAssetResolver } from "../../util/lambda";
import { SqsEventSource } from "aws-cdk-lib/aws-lambda-event-sources";
import { Topic } from "aws-cdk-lib/aws-sns";
import {
  AttributeType,
  StreamViewType,
  TableV2,
} from "aws-cdk-lib/aws-dynamodb";
import { RemovalPolicy } from "aws-cdk-lib";

export interface AcmeTopicProps {
  topicName?: string;
}

export class AcmeTopic extends Construct {
  public readonly outboxTable: TableV2;
  public readonly topicName: string;
  public readonly topic: Topic;
  public readonly deadLetterQueue: Queue;
  public readonly queue: Queue;
  public readonly messageRelayFunction: Function;

  constructor(scope: Construct, id: string, props?: AcmeTopicProps) {
    super(scope, id);

    this.topicName = props?.topicName ?? "default";

    this.topic = new Topic(this, `${this.node.id}-topic`, {
      topicName: `${this.node.id}-topic`,
    });

    this.outboxTable = new TableV2(this, `${this.node.id}-outbox-table`, {
      tableName: `${this.node.id}-outbox-table`,
      partitionKey: { name: "id", type: AttributeType.STRING },
      dynamoStream: StreamViewType.NEW_AND_OLD_IMAGES,
      removalPolicy: RemovalPolicy.DESTROY,
      timeToLiveAttribute: "ttl",
    });

    this.deadLetterQueue = new Queue(this, `${this.node.id}-dlq`, {
      queueName: `${this.node.id}-dlq`,
    });

    this.queue = new Queue(this, `${this.node.id}-queue`, {
      queueName: `${this.node.id}-queue`,
      deadLetterQueue: {
        queue: this.deadLetterQueue,
        maxReceiveCount: 3,
      },
    });

    this.messageRelayFunction = new Function(
      this,
      `${this.node.id}-relay-function`,
      {
        functionName: `${this.node.id}-relay-function`,
        code: zipAssetResolver("MessageRelay"),
        handler: createHandler("Acme", "MessageRelay"),
        runtime: Runtime.DOTNET_8,
      }
    );

    this.messageRelayFunction.addEventSource(new SqsEventSource(this.queue));
    this.messageRelayFunction.addEnvironment(
      "SNS_TOPIC_ARN",
      this.topic.topicArn
    );
    this.messageRelayFunction.addEnvironment(
      "OUTBOX_TABLE_NAME",
      this.outboxTable.tableName
    );

    this.queue.grantConsumeMessages(this.messageRelayFunction);
    this.topic.grantPublish(this.messageRelayFunction);

    const pipeRole = new Role(this, `${this.node.id}-role-pipe`, {
      roleName: `${this.node.id}-role-pipe`,
      assumedBy: new ServicePrincipal("pipes.amazonaws.com"),
    });

    new CfnPipe(this, "OutboxPipe", {
      name: `${this.node.id}-pipe`,
      roleArn: pipeRole.roleArn,
      source: this.outboxTable.tableStreamArn!,
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
                dynamodb: { NewImage: { topic: { S: [this.topicName] } } },
              }),
            },
          ],
        },
      },
      target: this.queue.queueArn,
    });
  }
}
