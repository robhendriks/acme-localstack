import { RemovalPolicy } from "aws-cdk-lib";
import { AttributeType, TableV2 } from "aws-cdk-lib/aws-dynamodb";
import { Construct } from "constructs";

export interface AcmeEntityDbProps {
  partitionKey: string;
  entityName: string;
}

export class AcmeEntityDb extends Construct {
  public readonly table: TableV2;
  public readonly entityName: string;

  constructor(scope: Construct, id: string, props: AcmeEntityDbProps) {
    super(scope, id);

    this.entityName = props.entityName;

    this.table = new TableV2(this, `${this.node.id}-table`, {
      tableName: `${this.node.id}-table`,
      partitionKey: { name: props.partitionKey, type: AttributeType.STRING },
      removalPolicy: RemovalPolicy.DESTROY,
    });
  }
}
