import { Construct } from "constructs";
import { Topic } from "aws-cdk-lib/aws-sns";
import { AcmeInbox } from "./acme-inbox";
import { AcmeOutbox } from "./acme-outbox";
import { SqsSubscription } from "aws-cdk-lib/aws-sns-subscriptions";
import { Role, ServicePrincipal } from "aws-cdk-lib/aws-iam";
import { CfnPipe } from "aws-cdk-lib/aws-pipes";

export interface AcmeTopicProps {
  topicName?: string;
}

export class AcmeTopic extends Construct {
  public readonly topicName: string;
  public readonly topic: Topic;

  public readonly outbox: AcmeOutbox;
  public readonly inbox: AcmeInbox;

  constructor(scope: Construct, id: string, props?: AcmeTopicProps) {
    super(scope, id);

    this.topicName = props?.topicName ?? "default";

    this.topic = new Topic(this, `${this.node.id}-topic`, {
      topicName: `${this.node.id}-topic`,
    });

    this.outbox = new AcmeOutbox(this, `${this.node.id}-outbox`, {
      topicName: this.topicName,
    });

    this.inbox = new AcmeInbox(this, `${this.node.id}-inbox`);

    // Subscribe inbox queue to SNS topic
    this.topic.addSubscription(new SqsSubscription(this.inbox.queue));

    // Configure SNS in outbox processor
    this.topic.grantPublish(this.outbox.processorFunction);

    this.outbox.processorFunction.addEnvironment(
      "SNS_TOPIC_ARN",
      this.topic.topicArn
    );
  }
}
