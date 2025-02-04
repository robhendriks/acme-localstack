import { Node } from "constructs";
import kebabCase from "kebab-case";

export function generateName(node: Node, ...paths: string[]): string {
  const segments = [...node.path.split("/"), ...paths];
  return segments.map((s) => kebabCase(s, false)).join("-");
}
