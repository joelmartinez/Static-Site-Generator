# Repo-level Copilot rules — interactive-flows

## Scaffold Flow
1. Ask for `scenario` (string).
2. Create `/interactive-flows/content/{scenario}`.
3. Inside that dir create
   * **nodes.json**     → `[]`
4. Always show a diff and wait for approval.

## Add Node
Append a node object (must validate against `/interactive-flows/schemas/flow-node.schema.json`) to the target *nodes.json*.

## Add Option
Insert one option object into `options[]` of the specified node.

## Validate
Run `node CodeCubeConsole/out/script/validate.js {scenario}` and print any errors.

**Never edit files outside `interactive-flows/` or `CodeCubeConsole/out/script/` unless explicitly requested.**