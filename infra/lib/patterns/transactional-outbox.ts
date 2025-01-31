import { RemovalPolicy } from "aws-cdk-lib";
import {
  TableV2,
  AttributeType,
  StreamViewType,
} from "aws-cdk-lib/aws-dynamodb";
import { SqsQueue } from "aws-cdk-lib/aws-events-targets";
import { IGrantable, Role, ServicePrincipal } from "aws-cdk-lib/aws-iam";
import { CfnPipe } from "aws-cdk-lib/aws-pipes";
import { Queue } from "aws-cdk-lib/aws-sqs";
import { StringParameter } from "aws-cdk-lib/aws-ssm";
import { Construct } from "constructs";

export interface TransactionalOutboxProps {}

export class TransactionalOutbox extends Construct {
  public table: TableV2;
  public param: StringParameter;
  public fqdn: string;

  constructor(scope: Construct, id: string, props?: TransactionalOutboxProps) {
    super(scope, id);

    this.fqdn = `${scope.node.id}-${this.node.id}`;

    this.table = new TableV2(this, "TransactionalOutboxTable", {
      tableName: `${this.fqdn}-Table`,
      partitionKey: { name: "id", type: AttributeType.STRING },
      removalPolicy: RemovalPolicy.DESTROY,
      dynamoStream: StreamViewType.NEW_AND_OLD_IMAGES,
    });

    this.param = new StringParameter(this, "TransactionalOutboxTableName", {
      parameterName: "/OutboxTable/TableName",
      stringValue: this.table.tableName,
    });
  }

  grant(grantable: IGrantable) {
    this.table.grantReadWriteData(grantable);
    this.param.grantRead(grantable);
  }
}

export interface TransactionalOutboxTopicProps {
  topicName?: string;
}

export class TransactionalOutboxTopic extends Construct {
  public queue: Queue;
  public fqdn: string;
  public topicName: string;

  private _pipeRole?: Role;
  private _pipe?: CfnPipe;

  constructor(
    scope: Construct,
    id: string,
    props?: TransactionalOutboxTopicProps
  ) {
    super(scope, id);

    this.fqdn = `${scope.node.id}-${this.node.id}`;

    this.topicName = props?.topicName ?? "default";

    this.queue = new Queue(this, "Queue", {
      queueName: `${this.fqdn}-Queue`,
    });
  }

  public connectTo(outbox: TransactionalOutbox) {
    this._pipeRole = new Role(this, "OutboxPipeRole", {
      roleName: `${this.fqdn}-PipeRole`,
      assumedBy: new ServicePrincipal("pipes.amazonaws.com"),
    });

    this._pipe = new CfnPipe(this, "OutboxPipe", {
      name: `${this.fqdn}-Pipe`,
      roleArn: this._pipeRole.roleArn,
      source: outbox.table.tableStreamArn!,
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
      target: this.queue.queueArn,
    });
  }
}
