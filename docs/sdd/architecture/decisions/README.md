# Architecture decisions

This section defines how to record new decisions that change stable backend boundaries. [`docs/legacy/adr/`](../../../legacy/README.md) holds historical ADRs; their numbers, stack, and wording do not establish current implementation without checking code and current documents.

A new ADR is needed for a decision with lasting consequences: module boundaries, deployment architecture, data model, or a substantial change in integration or security approach. Describe a decision limited to one feature first in its `design/` under [`specs/`](../../specs/README.md). Do not create an ADR for every package or local refactor.

For a new ADR, create a separate file with a unique number and short name. Include the date, status (`proposed`, `accepted`, `superseded`), context and factual sources, alternatives considered, chosen decision, consequences, and links to related specifications. When the decision changes, preserve the history: add a new ADR and mark the previous one as superseded. After acceptance, update the relevant [architecture](../README.md) overview documents and, if needed, the [standards](../../standards/README.md).

## Current ADRs

| ADR | Decision |
| --- | --- |
| [0001](0001-modular-monolith.md) | Modular monolith with one PostgreSQL schema per module |
| [0002](0002-one-shot-jobs-on-cloud-run.md) | 1C sync and migrations as one-shot jobs on Cloud Run Jobs |
| [0003](0003-cloudflare-worker-api-front-door.md) | Cloudflare Worker as the API front door |
| [0004](0004-contracts-and-business-folders.md) | Modules talk only through Contracts; folders follow the business |

Frontend decisions live in kedr-web: [Next.js public site](https://github.com/Weretik/kedr-web/blob/main/docs/architecture/site/adr/0001-nextjs-public-site.md) and [offline-first mobile app](https://github.com/Weretik/kedr-web/blob/main/docs/architecture/mobile/adr/0005-offline-first-local-database.md).
