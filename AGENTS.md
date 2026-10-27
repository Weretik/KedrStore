# AI navigation: KedrStore

This is a short entry point for working in this backend repository. Its code and documentation define the actual rules, architecture, tools, and commands. Read only the sources needed for the specific task. This file applies throughout the repository unless a nested `AGENTS.md` provides more specific instructions.

## Product

Kedr (ClassKedr) sells door hardware wholesale, with optional retail orders; it offers no fitting or repair services. 1C is the source of catalog, prices, stock, and counterparties; the backend imports from 1C and sends orders back. Authorization is not enforced yet: see the [current state](docs/sdd/architecture/security/authorization/roles-and-policies.md#current-state-authorization-not-yet-enforced) before touching access rules.

## Project context

- [Architecture](docs/sdd/architecture/README.md) — modules, layers, and dependency boundaries.
- [Standards](docs/sdd/standards/README.md) — stable rules for development, delivery, and verification.
- [Specifications](docs/sdd/specs/README.md) — feature requirements, decisions, tasks, and completion evidence.
- [Implemented feature map](docs/sdd/features/README.md) — navigation by module, structure, and code entry points.
- [Contracts](docs/sdd/contracts/README.md) — public API behavior and contract synchronization.
- [Operations documentation](docs/sdd/operations/README.md) — startup, configuration, migrations, seed/jobs, and diagnostics.
- [Glossary](docs/product/glossary.md) — stable business terms.

Identify the module and affected documents from the request and code; do not assume another backend project uses the same implementation. For task-specific context, navigate from the [feature map](docs/sdd/features/README.md) to the relevant feature description and code; read specifications, architecture, and standards only for the issues involved.

For a new or substantially changed feature, update the [feature map](docs/sdd/features/README.md) under the [delivery rules](docs/sdd/standards/delivery-rules.md); do not document an internal detail again if it does not affect the map.

## Finding context

1. Feature map → feature card → the code in its “Code structure” and the tests in its “Tests”.
2. Search code in `src/` and `tests/` first; search `docs/sdd/` only for the relevant section.
3. Open `docs/sdd/specs/**` only for decisions or history of that feature; do not read old task files or revisions by default. Paths in completed task files may be outdated.
4. Open `docs/legacy/` only when the user asks; it is not a current rule.

## Task routing

| Task | Skill | Project rules |
| --- | --- | --- |
| Specify a new or changed feature | `$backend-feature-spec` | [Delivery rules](docs/sdd/standards/delivery-rules.md) |
| Implement an accepted specification | `$backend-feature-delivery` | [Delivery rules](docs/sdd/standards/delivery-rules.md) |
| Multiple agents at the user's explicit request | `$backend-feature-orchestrator` | [Specifications and dependencies](docs/sdd/specs/README.md) |
| Code audit / systemic remediation plan | `$backend-code-audit` / `$backend-remediation-plan` | [Architecture](docs/sdd/architecture/README.md) and [standards](docs/sdd/standards/README.md) |
| HTTP/API and external contracts | `$backend-api-contract-sync` | [Contracts](docs/sdd/contracts/README.md) |
| Find database tables and relationships | — | [Database map](docs/sdd/architecture/data-model/README.md): start with the tree, then read only the relevant owner's file |
| Database schema, migrations, and seed data | `$backend-database-change` | [Database rules](docs/sdd/standards/database-rules.md) and [data operations](docs/sdd/operations/data/migrations-and-seeding.md) |
| Verify changes / hand off for review | `$backend-change-verification` / `$backend-pr-handoff` | [Testing rules](docs/sdd/standards/testing-rules.md) |
| Differences between code and documentation | `$backend-docs-sync` | [Standards index](docs/sdd/standards/README.md) and [feature map](docs/sdd/features/README.md) |

For a new feature, apply the [delivery rules](docs/sdd/standards/delivery-rules.md) and [backend code organization rules](docs/sdd/standards/backend-rules.md): plan the file-role map in the SDD first and compare it with actual code before handoff.

## Process priority

The user's direct request and this repository's rules define the scope of work. For feature specification, implementation, and verification, use the relevant `backend-*` skills and project documents as the primary process. Apply additional skills only for a needed method; they do not add repeated approval, mandatory agents, a worktree, or a separate TDD cycle on top of the project process. Run checks and tests according to this repository's rules and the task requirements.

## Scope and verification

Complete all ready tasks within the agreed feature, phase, scenario, or task scope without a separate instruction for each file; stop at the scope boundary, an unresolved product decision, or a concrete blocker. The detailed procedure is in the [delivery rules](docs/sdd/standards/delivery-rules.md). Choose checks according to the [testing rules](docs/sdd/standards/testing-rules.md) and the configuration of affected projects. For new identifiers, read the [ID strategy](docs/sdd/standards/identifier-strategy.md).

Starting `Host.Api` and working with the database can change state; before doing either, check the [project procedure for migrations and seed](docs/sdd/operations/data/migrations-and-seeding.md) and the permitted environment.

Make commits, push, create a branch or PR, or take other actions that change remote state only at the user's direct request. Preserve unrelated working-tree changes. All links here point only to files in this repository; skills are available globally by name, and this repository has no local copies.
