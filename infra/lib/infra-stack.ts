import { Stack, StackProps, Tags } from "aws-cdk-lib";
import { DomainName, HttpApi } from "aws-cdk-lib/aws-apigatewayv2";
import { EventBus } from "aws-cdk-lib/aws-events";
import { StringParameter } from "aws-cdk-lib/aws-ssm";
import { Construct } from "constructs";
import { HTTP_API_ID_PARAMETER_PATH } from "./util/http-api";

export class InfraStack extends Stack {
  public readonly httpApi: HttpApi;
  public readonly eventBus: EventBus;

  constructor(scope: Construct, id: string, props?: StackProps) {
    super(scope, id, props);

    this.httpApi = this.createHttpApi();
    this.eventBus = this.createEventBus();
  }

  private createHttpApi(): HttpApi {
    const httpApi = new HttpApi(this, "http-api", {
      apiName: "acme",
      createDefaultStage: false,
    });

    httpApi.addStage("dev", {
      stageName: "dev",
      autoDeploy: true,
    });

    Tags.of(httpApi).add("_custom_id_", "acme");

    new StringParameter(this, "param-http-api-id", {
      parameterName: HTTP_API_ID_PARAMETER_PATH,
      stringValue: httpApi.httpApiId,
    });

    return httpApi;
  }

  private createEventBus(): EventBus {
    const eventBus = new EventBus(this, "event-bus", {
      eventBusName: "acme",
    });

    return eventBus;
  }
}
