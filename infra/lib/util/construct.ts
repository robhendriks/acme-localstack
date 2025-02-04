import { Node } from "constructs";

export function generateName(node: Node, ...paths: string[]): string {
  const segments = [...node.path.split(/\//), ...paths];
  return segments.join("-");
}
