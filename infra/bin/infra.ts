#!/usr/bin/env node
import * as cdk from "aws-cdk-lib";
import { InfraStack } from "../lib/infra-stack";
import { OrderApiStack } from "../lib/order-api-stack";
import { FargateStack } from "../lib/fargate-stack";

const app = new cdk.App();

const infra = new InfraStack(app, "Acme-dev-Infra", {});

new OrderApiStack(app, "Acme-dev-OrderApi", {
  api: infra.api,
});

new FargateStack(app, "Acme-dev-FargateExample", {
  cluster: infra.cluster,
});
