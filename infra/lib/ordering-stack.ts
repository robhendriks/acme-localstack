import { Stack, StackProps } from "aws-cdk-lib";
import { Construct } from "constructs";
import { HttpMethod } from "aws-cdk-lib/aws-apigatewayv2";
import { AcmeFunction } from "./constructs/lambda/acme-function";
import { AcmeOutbox } from "./constructs/events/acme-outbox";

export class OrderingStack extends Stack {
  public outbox: AcmeOutbox;
  public createOrderFunction: AcmeFunction;
  public getOrderFunction: AcmeFunction;

  constructor(scope: Construct, id: string, props?: StackProps) {
    super(scope, id, props);

    this.outbox = new AcmeOutbox(this, `${this.node.id}-outbox`);

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
  }
}
