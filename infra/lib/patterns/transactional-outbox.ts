import { RemovalPolicy } from "aws-cdk-lib";
import {
  TableV2,
  AttributeType,
  StreamViewType,
} from "aws-cdk-lib/aws-dynamodb";
import { IGrantable } from "aws-cdk-lib/aws-iam";
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

  grantWrite(grantable: IGrantable) {
    this.table.grantWriteData(grantable);
  }
}
