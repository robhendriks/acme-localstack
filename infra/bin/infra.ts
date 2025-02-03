#!/usr/bin/env node
import * as cdk from "aws-cdk-lib";
import { InfraStack } from "../lib/infra-stack";
import { OrderingStack } from "../lib/ordering-stack";

const app = new cdk.App();

const stacks = (process.env.ACME_STACKS ?? "").split(",");

console.log("Stacks", stacks);

if (stacks.includes("infra")) {
  new InfraStack(app, "acme-infra", {
    stackName: "acme-infra",
  });
} else {
  console.info("skipping infra stack");
}

if (stacks.includes("ordering")) {
  new OrderingStack(app, "acme-ordering", {
    stackName: "acme-ordering",
  });
} else {
  console.info("skipping ordering stack");
}
