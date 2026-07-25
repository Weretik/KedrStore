# Phase 05: Swagger manual verification

**Status:** draft  
**Depends on:** phase 04  
**Blocks:** frontend handoff and completion

## Outcome

A developer can repeat a documented manual test through Swagger without hidden setup.

## Preconditions

- Host profile and URL:
- Environment:
- Required database/seed data:
- Required role/token, or explicit AllowAnonymous:
- HTTPS development certificate requirement:

## Steps

1. Start Host.Api with the exact command:
2. Open Swagger URL:
3. Select operation:
4. Enter request values:
5. Execute:
6. Record expected status/body:

## Cases

| Case | Request | Expected HTTP status | Expected result |
| --- | --- | --- | --- |
| Success | | | |
| Validation failure | | 400 | validation errors |
| Not found | | 404 | |
| Forbidden/unauthorized, if applicable | | 401/403 | |
| Duplicate/retry write, if applicable | | | |

## Acceptance criteria

- [ ] Every client-visible operation has a reproducible Swagger test.
- [ ] Error responses do not leak internals.
- [ ] OpenAPI parameters and response schema match the runtime response.

## Evidence

- Date/environment:
- Tester:
- Result:
- Remaining manual risks:
