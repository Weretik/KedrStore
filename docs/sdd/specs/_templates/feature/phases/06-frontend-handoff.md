# Phase 06: Frontend contract handoff

**Status:** draft  
**Depends on:** phases 04 and 05  
**Blocks:** feature completion for a frontend-consumed API

## Outcome

The frontend receives a stable, self-contained contract document and does not need to inspect backend handlers.

## Required contract document

Complete contracts/api-contract.md with:

- endpoint name, HTTP method and final URL;
- authentication/authorization requirement;
- route, query and body DTO fields, types, requiredness and defaults;
- response DTO fields, types, nullability and example;
- pagination, sorting, filtering and enum semantics;
- success and error statuses;
- idempotency/retry semantics for writes;
- compatibility/deprecation notice.

## Frontend integration notes

- Consumer/application:
- Data-table/form mapping:
- Empty/loading/error states:
- Locale/date/money formatting responsibility:
- Client-side validation that improves UX but does not replace backend validation:

## Acceptance criteria

- [ ] Contract has no reference to internal entity, DbContext or handler type.
- [ ] A frontend developer can call the API using only the contract document.
- [ ] Contract fields match the OpenAPI/runtime response verified in phase 05.

## Handoff record

- Contract version/date:
- Consumer owner:
- Open questions:
