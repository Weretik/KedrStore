# Backend feature SDD template

Use this template for a new business capability, CRUD resource or backend endpoint group. It creates one parent SDD and a separate SDD document for every implementation phase.

## Copy this structure

~~~text
docs/sdd/specs/<module>/<feature-slug>/
├── README.md                       copy template-feature.md
├── contracts/
│   └── api-contract.md             copy when frontend/external consumer exists
└── phases/
    ├── 01-domain.md
    ├── 02-infrastructure.md
    ├── 03-application.md
    ├── 04-api.md
    ├── 05-swagger-manual-test.md
    ├── 06-frontend-handoff.md
    └── 99-custom-phase.md          only for an additional independent phase
~~~

## Phase dependency

~~~text
01 Domain
   ↓
02 Infrastructure ──┐
                     ├──→ 03 Application → 04 API → 05 Swagger → 06 Frontend handoff
API contract ────────┘
~~~

The API contract may be drafted before implementation and refined through phases 03 and 04. Do not create empty phase files: copy only phases that apply, but record why a skipped phase is not needed in the parent README.

## Separation of responsibility

- README describes the feature goal, scope, scenarios, compatibility, ownership and the phase plan.
- contracts contains consumer-facing API facts only.
- phases contains implementation decisions, file plans, acceptance criteria and verification for that phase.
- Do not place implementation notes in the API contract and do not place the full frontend payload description in a handler-phase document.
