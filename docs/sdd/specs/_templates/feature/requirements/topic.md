# <feature name> — <business capability>

## Behavior

- <user action and observable system result>

## Business rules

### R-001 — <rule name>

<Rule stated without implementation details.>

## Acceptance scenarios

### SC-001 — <successful behavior>

**Covers:** R-001

Given <initial observable state>
And <relevant precondition>
When <actor action or external event>
Then <observable result>
And <additional observable result>

Examples, when boundaries or variants matter:

| Input/state | Expected result |
| --- | --- |
| <value> | <result> |

### SC-002 — <negative or boundary behavior>

**Covers:** R-001

Given <initial observable state>
When <action>
Then <observable rejection or unchanged state>

## Deferred scenarios

- <scenario intentionally outside this feature>

Scenario IDs are stable after acceptance. Describe behavior here; keep class
names, frameworks, database details, endpoints, and file paths in design,
contracts, and tasks.
