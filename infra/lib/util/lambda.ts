import { AssetCode, Code } from "aws-cdk-lib/aws-lambda";
import { resolve } from "path";

export function zipAssetResolver(projectName: string): AssetCode {
  return Code.fromAsset(resolve("../", "publish", `${projectName}.zip`), {});
}

export function createHandler(
  rootNamespace: string,
  projectName: string
): string {
  return `${rootNamespace}.${projectName}::${rootNamespace}.${projectName}.Function::FunctionHandler`;
}
