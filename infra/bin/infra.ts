#!/usr/bin/env node
import * as cdk from "aws-cdk-lib";
import { InfraStack } from "../lib/infra-stack";
import { OrderApiStack } from "../lib/order-api-stack";

const app = new cdk.App();

const infra = new InfraStack(app, "Acme-dev-Infra", {});

new OrderApiStack(app, "Acme-dev-OrderApi", { api: infra.api, bus: infra.bus });
