import * as cdk from "aws-cdk-lib";
import { Construct } from "constructs";
import * as lambda from "aws-cdk-lib/aws-lambda";
import * as apigateway from "aws-cdk-lib/aws-apigateway";
import * as dynamodb from "aws-cdk-lib/aws-dynamodb";
import * as ssm from "aws-cdk-lib/aws-ssm";
import * as events from "aws-cdk-lib/aws-events";
import path = require("path");
import { TransactionalOutbox } from "./patterns/transactional-outbox";
import { TransactionalInbox } from "./patterns/transactional-inbox";
import { TransactionalTopic } from "./patterns/transactional-topic";

class OrderApiConstruct extends Construct {
  public readonly handler: lambda.Function;

  public readonly topic: TransactionalTopic;
  public readonly inbox: TransactionalInbox;
  public readonly outbox: TransactionalOutbox;

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

    this.inbox = new TransactionalInbox(this, "Inbox");

    this.outbox = new TransactionalOutbox(this, "Outbox");
    this.outbox.grantWrite(this.handler);

    this.topic = new TransactionalTopic(this, "DefaultTopic", {
      inbox: this.inbox,
      outbox: this.outbox,
    });

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
    const integration = new apigateway.LambdaIntegration(this.handler);

    const orders = api.root.addResource("orders");
    orders.addMethod("POST", integration);

    const orderDetail = orders.addResource("{orderId}");
    orderDetail.addMethod("GET", integration);
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
