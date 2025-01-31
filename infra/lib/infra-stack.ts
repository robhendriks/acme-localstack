import * as cdk from "aws-cdk-lib";
import { Construct } from "constructs";
import * as apigateway from "aws-cdk-lib/aws-apigateway";
import * as events from "aws-cdk-lib/aws-events";
import path = require("path");

export class InfraStack extends cdk.Stack {
  public api: apigateway.RestApi;
  public bus: events.EventBus;

  constructor(scope: Construct, id: string, props?: cdk.StackProps) {
    super(scope, id, props);

    this.api = new apigateway.RestApi(this, "Api", {
      deploy: true,
      deployOptions: { stageName: "dev" },
    });

    this.bus = new events.EventBus(this, "EventBus", {
      eventBusName: "acme-dev",
    });
  }
}
