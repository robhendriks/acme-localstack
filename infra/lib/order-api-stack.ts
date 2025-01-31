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

class OrderApiConstruct extends Construct {
  public readonly handler: lambda.Function;
  public readonly ordersTable: dynamodb.TableV2;
  public readonly outboxTable: dynamodb.TableV2;

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

    // Outbox table
    this.outboxTable = new dynamodb.TableV2(this, "OutboxTable", {
      tableName: `${this.node.id}-Outbox`,
      partitionKey: { name: "id", type: dynamodb.AttributeType.STRING },
      removalPolicy: cdk.RemovalPolicy.DESTROY,
      dynamoStream: dynamodb.StreamViewType.NEW_AND_OLD_IMAGES,
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

    const outboxQueue = new sqs.Queue(this, "OutboxQueue", {
      queueName: `${this.node.id}-OutboxQueue`,
    });

    const pipeRole = new iam.Role(this, "OutboxPipeRole", {
      roleName: `${this.node.id}-OutboxPipeRole`,
      assumedBy: new iam.ServicePrincipal("pipes.amazonaws.com"),
    });

    const outboxPipe = new pipes.CfnPipe(this, "OutboxPipe", {
      name: `${this.node.id}-OutboxPipe`,
      roleArn: pipeRole.roleArn,
      source: this.outboxTable.tableStreamArn!,
      sourceParameters: {
        dynamoDbStreamParameters: {
          startingPosition: "LATEST",
          batchSize: 1,
        },
        filterCriteria: {
          filters: [{ pattern: JSON.stringify({ eventName: ["INSERT"] }) }],
        },
      },
      target: outboxQueue.queueArn,
    });

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
