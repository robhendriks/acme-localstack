import { Stack, StackProps } from "aws-cdk-lib";
import { Construct } from "constructs";
import { AcmeFunction } from "./patterns/acme-function";
import { HttpMethod } from "aws-cdk-lib/aws-apigatewayv2";
import { AcmeOutbox } from "./patterns/acme-outbox";

export class OrderingStack extends Stack {
  public createOrderFunction: AcmeFunction;
  public getOrderFunction: AcmeFunction;
  public outbox: AcmeOutbox;

  constructor(scope: Construct, id: string, props?: StackProps) {
    super(scope, id, props);

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

    this.outbox = new AcmeOutbox(this, `${this.node.id}-outbox`);
  }
}
