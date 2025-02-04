import { HttpApi, IHttpApi } from "aws-cdk-lib/aws-apigatewayv2";
import { StringParameter } from "aws-cdk-lib/aws-ssm";
import { Construct } from "constructs";

export const HTTP_API_ID_PARAMETER_PATH = "/acme/infra/http-api-id";

export function importHttpApi(scope: Construct): IHttpApi {
  const httpApiId = StringParameter.fromStringParameterName(
    scope,
    `${scope.node.id}-param-http-api-id`,
    "/acme/infra/http-api-id"
  ).stringValue;

  return HttpApi.fromHttpApiAttributes(scope, `${scope.node.id}-http-api`, {
    httpApiId,
  });
}
