import * as cdk from "aws-cdk-lib";
import { Construct } from "constructs";
import * as apigateway from "aws-cdk-lib/aws-apigateway";
import * as events from "aws-cdk-lib/aws-events";
import * as ec2 from "aws-cdk-lib/aws-ec2";
import * as ecs from "aws-cdk-lib/aws-ecs";
import path = require("path");

export class InfraStack extends cdk.Stack {
  public api: apigateway.RestApi;
  public bus: events.EventBus;
  public vpc: ec2.Vpc;
  public cluster: ecs.Cluster;

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

    this.vpc = new ec2.Vpc(this, "Acme-dev", {
      vpcName: "Acme-dev",
      ipAddresses: ec2.IpAddresses.cidr("10.0.0.0/16"),
    });

    this.cluster = new ecs.Cluster(this, "Cluster", {
      clusterName: "Acme-dev",
      vpc: this.vpc,
      enableFargateCapacityProviders: true,
    });
  }
}
