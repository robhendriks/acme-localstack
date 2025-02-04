import { Construct } from "constructs";
import { Topic } from "aws-cdk-lib/aws-sns";
import { AcmeInbox } from "./acme-inbox";
import { AcmeOutbox } from "./acme-outbox";
import { SqsSubscription } from "aws-cdk-lib/aws-sns-subscriptions";
import { Role, ServicePrincipal } from "aws-cdk-lib/aws-iam";
import { CfnPipe } from "aws-cdk-lib/aws-pipes";
import { generateName } from "../../util/construct";

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

    this.topic = new Topic(this, "topic", {
      topicName: generateName(this.node, "topic"),
    });

    this.outbox = new AcmeOutbox(this, "outbox", {
      topicName: this.topicName,
    });

    this.inbox = new AcmeInbox(this, "inbox");

    // Subscribe inbox queue to SNS topic
    this.topic.addSubscription(new SqsSubscription(this.inbox.queue));

    // Configure SNS in outbox processor
    this.topic.grantPublish(this.outbox.function);

    this.outbox.function.addEnvironment("SNS_TOPIC_ARN", this.topic.topicArn);
  }
}
