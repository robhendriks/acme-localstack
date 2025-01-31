import * as cdk from "aws-cdk-lib";
import { Construct } from "constructs";
import * as lambda from "aws-cdk-lib/aws-lambda";
import * as apigateway from "aws-cdk-lib/aws-apigateway";
import * as dynamodb from "aws-cdk-lib/aws-dynamodb";
import * as ssm from "aws-cdk-lib/aws-ssm";
import path = require("path");

class OrderApiConstruct extends Construct {
  public readonly handler: lambda.Function;
  public readonly ordersTable: dynamodb.TableV2;
  public readonly outboxTable: dynamodb.TableV2;

  constructor(scope: Construct, id: string, api: apigateway.RestApi) {
    super(scope, id);

    this.handler = new lambda.Function(this, "OrderApiFunction", {
      runtime: lambda.Runtime.DOTNET_8,
      code: lambda.Code.fromAsset(
        path.resolve("../", "publish", "OrderApi.zip")
      ),
      handler: "Acme.OrderApi",
    });

    this.ordersTable = new dynamodb.TableV2(this, "OrderTable", {
      partitionKey: { name: "id", type: dynamodb.AttributeType.STRING },
      removalPolicy: cdk.RemovalPolicy.DESTROY,
    });

    // Orders table
    const ordersTableNameParam = new ssm.StringParameter(
      this,
      "OrderTableName",
      {
        parameterName: "/OrderTable/TableName",
        stringValue: this.ordersTable.tableName,
      }
    );

    ordersTableNameParam.grantRead(this.handler);

    // Outbox table
    this.outboxTable = new dynamodb.TableV2(this, "OutboxTable", {
      partitionKey: { name: "id", type: dynamodb.AttributeType.STRING },
      removalPolicy: cdk.RemovalPolicy.DESTROY,
    });

    const outboxTableNameParam = new ssm.StringParameter(
      this,
      "OutboxTableName",
      {
        parameterName: "/OutboxTable/TableName",
        stringValue: this.outboxTable.tableName,
      }
    );

    outboxTableNameParam.grantRead(this.handler);

    this.addRoutes(api);
  }

  private addRoutes(api: apigateway.RestApi) {
    const v1 = api.root.addResource("v1");
    const orders = v1.addResource("orders");

    orders.addMethod("POST", new apigateway.LambdaIntegration(this.handler));
  }
}

interface OrderApiProps extends cdk.StackProps {
  api: apigateway.RestApi;
}

export class OrderApiStack extends cdk.Stack {
  constructor(scope: Construct, id: string, props: OrderApiProps) {
    super(scope, id, props);

    new OrderApiConstruct(this, "OrderApiConstruct", props.api);
  }
}
