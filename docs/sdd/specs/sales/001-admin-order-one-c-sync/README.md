# 001 — Admin order and 1C synchronization

**Module:** Sales  
**Type:** feature  
**Status:** draft  
**Owner:** Sales team  
**Created:** 2026-08-17

Manager-created orders are a separate Sales capability. A manager creates an order for an existing counterparty; the order is stored locally and then delivered to 1C reliably in the background. This feature does not alter the anonymous public quick-order flow in Catalog or its Telegram/Excel notification.

## Goal and scope

- [Overview and boundaries](requirements/overview.md)
- [Manager order creation](requirements/manager-order-creation.md)
- [1C delivery](requirements/one-c-delivery.md)

## Technical design

- [Domain](design/domain.md)
- [Infrastructure](design/infrastructure.md)
- [Data model](data-model.md)

## Delivery

- [HTTP and SOAP contract](contracts/api-contract.md)
- [Specification readiness](checklist/spec-readiness.md)
- [Delivery readiness](checklist/delivery-readiness.md)

## AI implementation tasks

The specification defines concrete tasks for Phases 00–05. Execute them in order and keep the specification-readiness checklist complete before changing code.

- [00 — Readiness](tasks/00-readiness.md)
- [01 — Domain planning](tasks/01-domain.md)
- [02 — Infrastructure planning](tasks/02-infrastructure.md)
- [03 — Application planning](tasks/03-application.md)
- [04 — API planning](tasks/04-api.md)
- [05 — Background delivery job](tasks/05-jobs.md)
- [06 — Verification](tasks/06-verification.md)

## Related durable documentation

- [1C integration boundary](../../../architecture/integrations/one-c/README.md)
- [Backend standards](../../../standards/README.md)
- [Sales → 1C delivery instruction](../../../../../SALES_1C_SYNC_INSTRUCTION.txt)

## Change log

- 2026-08-19 — Added durable storage of the 1C-created document number after an accepted SOAP write. A future admin order read API must expose it to the frontend.

- 2026-08-17 — Initial draft. Public Catalog quick orders are explicitly excluded; the existing `CreateSiteRequest` SOAP operation is the intended 1C write operation.
