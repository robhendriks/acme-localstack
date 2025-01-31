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
      restApiName: "Acme-dev",
      deploy: true,
      deployOptions: { stageName: "dev" },
    });

    cdk.Tags.of(this.api).add("_custom_id_", "acme");

    this.bus = new events.EventBus(this, "EventBus", {
      eventBusName: "Acme-dev",
    });
  }
}
