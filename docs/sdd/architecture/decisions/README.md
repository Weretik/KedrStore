# Architecture decisions

This section defines how to record new decisions that change stable backend boundaries. As of 2026-10-15, this directory contains no current ADRs. [`docs/legacy/adr/`](../../../legacy/README.md) holds historical ADRs; their numbers, stack, and wording do not establish current implementation without checking code and current documents.

A new ADR is needed for a decision with lasting consequences: module boundaries, deployment architecture, data model, or a substantial change in integration or security approach. Describe a decision limited to one feature first in its `design/` under [`specs/`](../../specs/README.md). Do not create an ADR for every package or local refactor.

For a new ADR, create a separate file with a unique number and short name. Include the date, status (`proposed`, `accepted`, `superseded`), context and factual sources, alternatives considered, chosen decision, consequences, and links to related specifications. When the decision changes, preserve the history: add a new ADR and mark the previous one as superseded. After acceptance, update the relevant [architecture](../README.md) overview documents and, if needed, the [standards](../../standards/README.md).
