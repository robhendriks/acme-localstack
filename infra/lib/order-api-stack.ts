import * as cdk from "aws-cdk-lib";
import { Construct } from "constructs";
import * as lambda from "aws-cdk-lib/aws-lambda";
import * as apigateway from "aws-cdk-lib/aws-apigateway";
import * as dynamodb from "aws-cdk-lib/aws-dynamodb";
import * as ssm from "aws-cdk-lib/aws-ssm";
import * as events from "aws-cdk-lib/aws-events";
import * as sqs from "aws-cdk-lib/aws-sqs";
import * as pipes from "aws-cdk-lib/aws-pipes";
import * as iam from "aws-cdk-lib/aws-iam";
import path = require("path");
import {
  TransactionalOutbox,
  TransactionalOutboxTopic,
} from "./patterns/transactional-outbox";

class OrderApiConstruct extends Construct {
  public readonly handler: lambda.Function;
  public readonly outbox: TransactionalOutbox;
  public readonly defaultTopic: TransactionalOutboxTopic;
  public readonly ordersTable: dynamodb.TableV2;

  constructor(
    scope: Construct,
    id: string,
    api: apigateway.RestApi,
    bus: events.EventBus
  ) {
    super(scope, id);

    this.handler = new lambda.Function(this, "OrderApiFunction", {
      functionName: `${this.node.id}-Function`,
      runtime: lambda.Runtime.DOTNET_8,
      code: lambda.Code.fromAsset(
        path.resolve("../", "publish", "OrderApi.zip")
      ),
      handler: "Acme.OrderApi",
    });

    this.outbox = new TransactionalOutbox(this, "Outbox");
    this.outbox.grant(this.handler);

    this.defaultTopic = new TransactionalOutboxTopic(this, "DefaultTopic");
    this.defaultTopic.connectTo(this.outbox);

    this.ordersTable = new dynamodb.TableV2(this, "OrderTable", {
      tableName: `${this.node.id}-Orders`,
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
  bus: events.EventBus;
}

export class OrderApiStack extends cdk.Stack {
  constructor(scope: Construct, id: string, props: OrderApiProps) {
    super(scope, id, props);

    new OrderApiConstruct(this, "OrderApi-dev", props.api, props.bus);
  }
}
