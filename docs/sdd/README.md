# KedrStore backend SDD

This is the entry point to current backend documentation. Verify implementation details against repository code and configuration; specifications may describe planned changes, so their status does not replace checking current behavior.

| Question | Where to look |
| --- | --- |
| Which modules, layers, hosts, runtime flows, and integrations exist? | [Architecture](architecture/README.md) |
| Which technologies, direct NuGet dependencies, and tools are declared? | [Stack](architecture/platform/technology-stack.md), [package catalog](architecture/platform/nuget-packages.md), and [tooling](architecture/platform/tooling-and-delivery.md) |
| Which stable rules govern code, API, database, security, testing, and delivery? | [Standards](standards/README.md) |
| How do I start the API, prepare the database, and run jobs? | [Operations](operations/README.md) |
| Where are versioned API contracts and frontend integration rules? | [Contracts](contracts/README.md) |
| Where are requirements, design decisions, and evidence for a feature? | [Specifications](specs/README.md) |
| Which features are implemented, and where are their code and data? | [Feature map](features/README.md) |
| How do I distinguish a current architecture decision from a historical ADR? | [Decisions](architecture/decisions/README.md) |

Architecture describes the verified current state and stable boundaries. Standards contain current normative rules. `features/` shows the structure of current features, while `specs/` records the intent, progress, and evidence of a specific change. Historical material in [`docs/legacy/`](../legacy/README.md) is not automatically a current rule.

**Verification boundary:** The package inventory and startup behavior were checked against the repository on 2026-10-15. After changes to code, manifests, or deployment workflows, the relevant documents must be checked again.
