---
name: kedrstore-api-contract-sync
description: "Use when adding, changing, validating, or handing off a KedrStore public HTTP operation and its versioned OpenAPI contract."
---

# KedrStore API Contract Sync

Підтримуй human contract, versioned OpenAPI, runtime transport, tests і consumer handoff узгодженими.

## Workflow

1. Прочитай feature `contracts/api-contract.md`, `docs/sdd/contracts/README.md`, API/security standards і affected controller/use case.
2. До transport implementation, коли практично, узгодь route, method, stable operationId, DTO, status/error shapes, authorization, pagination, idempotency і compatibility.
3. Створи або онови `docs/sdd/contracts/<module>/<feature>.openapi.yaml`; aggregate `docs/sdd/contracts/openapi.yaml` має посилатися на нього через `$ref`, без дублювання operations.
4. Додай focused Red contract/API tests, реалізуй transport через explicit request models, Mediator і established Ardalis.Result mapping.
5. Перевір runtime status, serialization, authorization і examples проти OpenAPI. За breaking change задокументуй affected consumers, migration та rollout.
6. Онови feature traceability і frontend handoff. Не оголошуй endpoint public лише через наявність OpenAPI.

## Guardrails

- Не expose entities, persistence types, secrets або internal exceptions.
- Не вигадуй consumer requirements або handwritten schema, коли canonical behavior невідоме.
- Не редагуй consumer repository без явної авторизації.

## Example

`Use $kedrstore-api-contract-sync to add and verify the catalog product-archive operation.`
