import * as cdk from "aws-cdk-lib";
import * as ecs from "aws-cdk-lib/aws-ecs";
import { Queue } from "aws-cdk-lib/aws-sqs";
import { Construct } from "constructs";
import path = require("path");

interface FargateStackProps extends cdk.StackProps {
  cluster: ecs.Cluster;
}

export class FargateStack extends cdk.Stack {
  constructor(scope: Construct, id: string, props: FargateStackProps) {
    super(scope, id, props);

    const queue = new Queue(this, "FargateExampleQueue", {
      queueName: "FargateExample-Queue",
    });

    const taskDefinition = new ecs.FargateTaskDefinition(
      this,
      "FargateExampleTaskDefinition-dev",
      {
        family: "FargateExample-dev",
        memoryLimitMiB: 512,
        cpu: 256,
        runtimePlatform: {
          cpuArchitecture: ecs.CpuArchitecture.X86_64,
          operatingSystemFamily: ecs.OperatingSystemFamily.LINUX,
        },
      }
    );

    const container = taskDefinition.addContainer(
      "FargateExampleContainer-dev",
      {
        containerName: "FargateExample-dev",
        healthCheck: {
          command: [
            "CMD-SHELL",
            "curl --fail http://localhost/health} || exit 1",
          ],
          startPeriod: cdk.Duration.seconds(10),
        },
        environment: {
          SQS_QUEUE_URL: queue.queueUrl,
        },
        image: ecs.ContainerImage.fromAsset(
          path.resolve("../", "publish", "FargateExample")
        ),
      }
    );

    container.addPortMappings({
      appProtocol: ecs.AppProtocol.http,
      containerPort: 80,
      name: "http",
    });

    new ecs.FargateService(this, "FargateExample-dev", {
      cluster: props.cluster,
      serviceName: "FargateExample-dev",
      taskDefinition: taskDefinition,
      enableECSManagedTags: true,
      enableExecuteCommand: true,
    });
  }
}
