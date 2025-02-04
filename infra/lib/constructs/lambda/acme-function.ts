import {
  HttpApi,
  HttpMethod,
  HttpRoute,
  HttpRouteKey,
  IHttpApi,
} from "aws-cdk-lib/aws-apigatewayv2";
import { HttpLambdaIntegration } from "aws-cdk-lib/aws-apigatewayv2-integrations";
import { AssetCode, Code, Function, Runtime } from "aws-cdk-lib/aws-lambda";
import { Construct } from "constructs";
import { resolve } from "path";
import { importHttpApi } from "../../util/http-api";
import { AcmeOutbox } from "../events/acme-outbox";
import { StringParameter } from "aws-cdk-lib/aws-ssm";

export interface AcmeFunctionProps {
  projectName: string;
  handler?: string;
  runtime?: Runtime;
}

function zipAssetResolver(projectName: string): AssetCode {
  return Code.fromAsset(resolve("../", "publish", `${projectName}.zip`), {});
}

function createHandler(rootNamespace: string, projectName: string): string {
  return `${rootNamespace}.${projectName}::${rootNamespace}.${projectName}.Function::FunctionHandler`;
}

export class AcmeFunction extends Construct {
  public readonly function: Function;

  private _httpApi?: IHttpApi;
  private _httpApiIntegration?: HttpLambdaIntegration;

  constructor(scope: Construct, id: string, props: AcmeFunctionProps) {
    super(scope, id);

    this.function = new Function(this, `${this.node.id}-function`, {
      functionName: `${this.node.id}-function`,
      code: zipAssetResolver(props.projectName),
      handler: props.handler ?? createHandler("Acme", props.projectName),
      runtime: props.runtime ?? Runtime.DOTNET_8,
    });

    this.function.addEnvironment("ACME_APPLICATION", this.node.id);
  }

  private getHttpApi(): IHttpApi {
    return (this._httpApi ??= importHttpApi(this));
  }

  private getHttpIntegration(): HttpLambdaIntegration {
    return (this._httpApiIntegration ??= new HttpLambdaIntegration(
      `${this.node.id}-integration`,
      this.function
    ));
  }

  public addRoute(id: string, path: string, method: HttpMethod): AcmeFunction {
    new HttpRoute(this, `${this.node.id}-route-${id}}`, {
      httpApi: this.getHttpApi(),
      routeKey: HttpRouteKey.with(path, method),
      integration: this.getHttpIntegration(),
    });

    return this;
  }

  public addOutbox(outbox: AcmeOutbox): AcmeFunction {
    outbox.table.grantFullAccess(this.function);

    new StringParameter(this, `${this.node.id}-param-outbox-table-name`, {
      parameterName: `/${this.node.id}/Outbox/TableName`,
      stringValue: outbox.table.tableName,
    });

    return this;
  }
}
