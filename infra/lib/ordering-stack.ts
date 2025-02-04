import { Stack, StackProps } from "aws-cdk-lib";
import { Construct } from "constructs";
import { HttpMethod } from "aws-cdk-lib/aws-apigatewayv2";
import { AcmeFunction } from "./constructs/lambda/acme-function";
import { AcmeOutbox } from "./constructs/events/acme-outbox";
import { AcmeEntityDb } from "./constructs/storage/acme-entity-db";
import { AcmeTopic } from "./constructs/events/acme-topic";

export class OrderingStack extends Stack {
  public outbox: AcmeOutbox;
  public topic: AcmeTopic;
  public orderTable: AcmeEntityDb;
  public createOrderFunction: AcmeFunction;
  public getOrderFunction: AcmeFunction;

  constructor(scope: Construct, id: string, props?: StackProps) {
    super(scope, id, props);

    this.outbox = new AcmeOutbox(this, `${this.node.id}-outbox`);
    this.topic = new AcmeTopic(this, `${this.node.id}-topic-default`);

    this.topic.connectOutbox(this.outbox);

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

    this.createOrderFunction.addOutbox(this.outbox);
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
  }
}
