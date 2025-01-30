import * as cdk from "aws-cdk-lib";
import { Construct } from "constructs";
import * as lambda from "aws-cdk-lib/aws-lambda";
import * as apigateway from "aws-cdk-lib/aws-apigateway";
import * as dynamodb from "aws-cdk-lib/aws-dynamodb";
import * as ssm from "aws-cdk-lib/aws-ssm";
import path = require("path");

class OrderApiConstruct extends Construct {
  public readonly handler: lambda.Function;
  public readonly table: dynamodb.TableV2;

  constructor(scope: Construct, id: string, api: apigateway.RestApi) {
    super(scope, id);

    this.handler = new lambda.Function(this, "OrderApiFunction", {
      runtime: lambda.Runtime.DOTNET_8,
      code: lambda.Code.fromAsset(
        path.resolve("../", "publish", "OrderApi.zip")
      ),
      handler: "Acme.OrderApi",
    });

    this.table = new dynamodb.TableV2(this, "Orders", {
      partitionKey: { name: "id", type: dynamodb.AttributeType.STRING },
      removalPolicy: cdk.RemovalPolicy.DESTROY,
    });

    const tableNameParam = new ssm.StringParameter(this, "OrderTable", {
      parameterName: "/OrderTable/TableName",
      stringValue: this.table.tableName,
    });

    tableNameParam.grantRead(this.handler);

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
