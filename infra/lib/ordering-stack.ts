import { Stack, StackProps } from "aws-cdk-lib";
import { Construct } from "constructs";
import { HttpMethod } from "aws-cdk-lib/aws-apigatewayv2";
import { AcmeFunction } from "./constructs/lambda/acme-function";
import { AcmeEntityDb } from "./constructs/storage/acme-entity-db";
import { AcmeTopic } from "./constructs/events/acme-topic";

export class OrderingStack extends Stack {
  public topic: AcmeTopic;
  public orderTable: AcmeEntityDb;
  public createOrderFunction: AcmeFunction;
  public getOrderFunction: AcmeFunction;
  public orderRequestedProcessorFunction: AcmeFunction;

  constructor(scope: Construct, id: string, props?: StackProps) {
    super(scope, id, props);

    this.topic = new AcmeTopic(this, `${this.node.id}-topic-default`);

    this.orderTable = new AcmeEntityDb(this, `${this.node.id}-order-db`, {
      partitionKey: "id",
      entityName: "order",
    });

    this.createOrderFunction = new AcmeFunction(
      this,
      `${this.node.id}-create-order`,
      {
        projectName: "CreateOrder",
      }
    );

    this.createOrderFunction.addRoute(
      "create-order",
      "/orders",
      HttpMethod.POST
    );

    this.createOrderFunction.addOutbox(this.topic);
    this.createOrderFunction.addEntityDb(this.orderTable);

    this.getOrderFunction = new AcmeFunction(
      this,
      `${this.node.id}-get-order`,
      { projectName: "GetOrder" }
    );

    this.getOrderFunction.addRoute(
      "get-order",
      "/orders/{orderId}",
      HttpMethod.GET
    );

    this.getOrderFunction.addEntityDb(this.orderTable);

    this.orderRequestedProcessorFunction = new AcmeFunction(
      this,
      `${this.node.id}-order-requested-processor`,
      {
        projectName: "OrderRequestedProcessor",
      }
    );

    this.orderRequestedProcessorFunction.addQueue();
  }
}
