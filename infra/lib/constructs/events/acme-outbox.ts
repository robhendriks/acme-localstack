import { RemovalPolicy } from "aws-cdk-lib";
import {
  AttributeType,
  StreamViewType,
  TableV2,
} from "aws-cdk-lib/aws-dynamodb";
import { Construct } from "constructs";

export class AcmeOutbox extends Construct {
  public readonly table: TableV2;

  constructor(scope: Construct, id: string) {
    super(scope, id);

    this.table = new TableV2(this, `${this.node.id}-table`, {
      tableName: `${this.node.id}-table`,
      partitionKey: { name: "id", type: AttributeType.STRING },
      dynamoStream: StreamViewType.NEW_AND_OLD_IMAGES,
      removalPolicy: RemovalPolicy.DESTROY,
    });
  }
}
