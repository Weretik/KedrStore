# Architecture decisions

This section defines how to record new decisions that change stable backend boundaries. [`docs/legacy/adr/`](../../../legacy/README.md) holds historical ADRs; their numbers, stack, and wording do not establish current implementation without checking code and current documents.

A new ADR is needed for a decision with lasting consequences: module boundaries, deployment architecture, data model, or a substantial change in integration or security approach. Describe a decision limited to one feature first in its `design/` under [`specs/`](../../specs/README.md). Do not create an ADR for every package or local refactor.

For a new ADR, create a separate file with a unique number and short name. Include the date, status (`proposed`, `accepted`, `superseded`), context and factual sources, alternatives considered, chosen decision, consequences, and links to related specifications. When the decision changes, preserve the history: add a new ADR and mark the previous one as superseded. After acceptance, update the relevant [architecture](../README.md) overview documents and, if needed, the [standards](../../standards/README.md).

## Current ADRs

| ADR | Decision | Status |
| --- | --- | --- |
| [0001](0001-modular-monolith.md) | Modular monolith with one PostgreSQL schema per module | accepted |
| [0002](0002-one-shot-jobs-on-cloud-run.md) | 1C sync and migrations as one-shot jobs on Cloud Run Jobs | accepted; per-job schedules superseded by 0009 |
| [0003](0003-cloudflare-worker-api-front-door.md) | Cloudflare Worker as the API front door | accepted |
| [0004](0004-contracts-and-business-folders.md) | Modules talk only through Contracts; folders follow the business | accepted |
| [0005](0005-use-cases-through-mediator.md) | Every use case is a command or query sent through Mediator | accepted |
| [0006](0006-result-and-typed-errors.md) | Business rules return a Result with a typed error; exceptions only for the unexpected | accepted |
| [0007](0007-cookie-session-and-mobile-tokens.md) | Cookie session for browsers, bearer and refresh tokens for the mobile app | accepted |
| [0008](0008-transactional-outbox.md) | Side effects leave through a transactional outbox per module | accepted |
| [0009](0009-scheduler-tick-for-background-tasks.md) | One scheduler tick runs the outbox and every due background task | accepted |
| [0010](0010-keyless-deploy-to-cloud-run.md) | Keyless deploy from GitHub Actions to Cloud Run | accepted |
| [0011](0011-media-library-on-cdn.md) | Pictures live on the CDN; the Media module keeps a library of them by id | accepted |
| [0012](0012-folder-axiom.md) | A code folder holds at most six files | accepted |

Frontend decisions live in kedr-web: [workspace ADRs](https://github.com/Weretik/kedr-web/blob/main/docs/architecture/adr/README.md), [Next.js public site](https://github.com/Weretik/kedr-web/blob/main/docs/architecture/site/adr/0001-nextjs-public-site.md) and [offline-first mobile app](https://github.com/Weretik/kedr-web/blob/main/docs/architecture/mobile/adr/0005-offline-first-local-database.md). The whole system is drawn in the [C4 diagrams](../c4/README.md).

## Key decisions that must have an ADR

Every backend keeps an accepted ADR for each area below. A full architecture audit checks this list and reports each missing or outdated ADR as a finding; it also checks that the [C4 diagrams](../c4/README.md) match the code.

| Area | ADR here |
| --- | --- |
| Architecture style and module boundaries, data ownership (schema per module) | 0001, 0004 |
| Communication between modules | 0004 |
| Use-case shape (CQRS, mediator, endpoints) | 0005 |
| Errors and validation | 0006 |
| Authentication and authorization | 0007 |
| Side effects and integration messages | 0008 |
| Background work and scheduling | 0002, 0009 |
| Database migrations and their order in a release | 0002 |
| Deployment, hosting and secrets | 0010 |
| Network edge (proxy, domains, cookies) | 0003 |
| Files and media storage | 0011 |
| Code organization (folders) | 0004, 0012 |
| Main external systems (1C) | 0002 |

A new decision of this kind (a new module, a new external system, a new way to run work or to deploy) gets an ADR in the same change, and the C4 diagrams are updated with it.
