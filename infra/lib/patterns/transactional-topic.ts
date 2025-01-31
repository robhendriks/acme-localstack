import { Queue } from "aws-cdk-lib/aws-sqs";
import { Construct } from "constructs";
import { TransactionalInbox } from "./transactional-inbox";
import { TransactionalOutbox } from "./transactional-outbox";
import { Topic } from "aws-cdk-lib/aws-sns";
import path = require("path");
import { Code, Runtime, Function } from "aws-cdk-lib/aws-lambda";
import {
  SnsEventSource,
  SqsEventSource,
} from "aws-cdk-lib/aws-lambda-event-sources";
import { Role, ServicePrincipal } from "aws-cdk-lib/aws-iam";
import { CfnPipe } from "aws-cdk-lib/aws-pipes";
import { SqsSubscription } from "aws-cdk-lib/aws-sns-subscriptions";

export interface TransactionalTopicProps {
  topicName?: string;
  fifo?: boolean;
  inbox: TransactionalInbox;
  outbox: TransactionalOutbox;
}

export class TransactionalTopic extends Construct {
  public fqdn: string;
  public topicName: string;

  public topic: Topic;

  public outboxQueue: Queue;
  public inboxQueue: Queue;

  public outboxFunction: Function;
  public inboxFunction: Function;

  constructor(scope: Construct, id: string, props: TransactionalTopicProps) {
    super(scope, id);

    this.fqdn = `${scope.node.id}-${this.node.id}`;
    this.topicName = props?.topicName ?? "default";

    this.topic = new Topic(this, "Topic", {
      topicName: `${this.fqdn}-${this.topicName}`,
      fifo: props?.fifo,
    });

    this.inboxQueue = new Queue(this, "InboxQueue", {
      queueName: `${this.fqdn}-InboxQueue`,
    });

    this.outboxQueue = new Queue(this, "OutboxQueue", {
      queueName: `${this.fqdn}-OutboxQueue`,
    });

    this.outboxFunction = this.createOutboxFunction();
    props.outbox.table.grantReadData(this.outboxFunction);

    this.inboxFunction = this.createInboxFunction();
    props.inbox.table.grantReadWriteData(this.inboxFunction);

    const pipeRole = new Role(this, "PipeRole", {
      roleName: `${this.fqdn}-PipeRole`,
      assumedBy: new ServicePrincipal("pipes.amazonaws.com"),
    });

    // Create outbox pipe
    const pipe = new CfnPipe(this, "OutboxPipe", {
      name: `${this.fqdn}-Pipe`,
      roleArn: pipeRole.roleArn,
      source: props.outbox.table.tableStreamArn!,
      sourceParameters: {
        dynamoDbStreamParameters: {
          startingPosition: "LATEST",
          batchSize: 1,
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
      target: this.outboxQueue.queueArn,
    });

    this.topic.addSubscription(new SqsSubscription(this.inboxQueue));
  }

  private createOutboxFunction(): Function {
    const fn = new Function(this, "OutboxProcessor", {
      functionName: `${this.fqdn}-OutboxProcessor`,
      runtime: Runtime.DOTNET_8,
      code: Code.fromAsset(
        path.resolve("../", "publish", "OutboxProcessor.zip")
      ),
      handler:
        "Acme.OutboxProcessor::Acme.OutboxProcessor.Function::FunctionHandler",
    });

    fn.addEnvironment("SNS_TOPIC_ARN", this.topic.topicArn);
    fn.addEnvironment("SNS_TOPIC_NAME", this.topic.topicName);

    fn.addEventSource(new SqsEventSource(this.outboxQueue));

    return fn;
  }

  private createInboxFunction(): Function {
    const fn = new Function(this, "InboxProcessor", {
      functionName: `${this.fqdn}-InboxProcessor`,
      runtime: Runtime.DOTNET_8,
      code: Code.fromAsset(
        path.resolve("../", "publish", "InboxProcessor.zip")
      ),
      handler:
        "Acme.InboxProcessor::Acme.InboxProcessor.Function::FunctionHandler",
    });

    fn.addEventSource(new SqsEventSource(this.inboxQueue));

    return fn;
  }
}
