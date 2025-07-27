# Repo-level Copilot rules — interactive-flows

## Scaffold Flow
1. Ask for `scenario` (string) and `locale` (BCP-47, default `en-US`).
2. Create `/interactive-flows/content/{scenario}/{locale}`.
3. Inside that dir create
   * **nodes.json**     → `[]`
   * **outcomes.json**  → `[]`
   * **strings.json**   → `{}`
4. Always show a diff and wait for approval.

## Add Node
Append a node object (must validate against `/interactive-flows/schemas/flow-node.schema.json`) to the target *nodes.json*.

## Add Option
Insert one option object into `options[]` of the specified node.

## Link Outcome
Set `outcomeId` on the node and (if necessary) change `"type"` to `"outcome"`.

## Validate
Run `node CodeCubeConsole/out/script/validate.js {scenario} {locale}` and print any errors.

**Never edit files outside `interactive-flows/` or `CodeCubeConsole/out/script/` unless explicitly requested.**