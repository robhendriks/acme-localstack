import { Queue } from "aws-cdk-lib/aws-sqs";
import { Construct } from "constructs";
import { AcmeOutbox } from "./acme-outbox";
import { Role, ServicePrincipal } from "aws-cdk-lib/aws-iam";
import { CfnPipe } from "aws-cdk-lib/aws-pipes";

export interface AcmeTopicProps {
  topicName?: string;
}

export class AcmeTopic extends Construct {
  public readonly topicName: string;
  public readonly deadLetterQueue: Queue;
  public readonly queue: Queue;

  constructor(scope: Construct, id: string, props?: AcmeTopicProps) {
    super(scope, id);

    this.topicName = props?.topicName ?? "default";

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
  }

  public connectOutbox(outbox: AcmeOutbox): AcmeTopic {
    const pipeRole = new Role(this, `${this.node.id}-role-pipe`, {
      roleName: `${this.node.id}-role-pipe`,
      assumedBy: new ServicePrincipal("pipes.amazonaws.com"),
    });

    new CfnPipe(this, "OutboxPipe", {
      name: `${this.node.id}-pipe`,
      roleArn: pipeRole.roleArn,
      source: outbox.table.tableStreamArn!,
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

    return this;
  }
}
