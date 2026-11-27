# AI navigation: KedrStore

This is a short entry point for working in this backend repository. Its code and documentation define the actual rules, architecture, tools, and commands. Read only the sources needed for the specific task. This file applies throughout the repository unless a nested `AGENTS.md` provides more specific instructions.

## Product

Kedr sells door hardware wholesale (with optional retail orders), not services such as fitting or repair. Browsers sign in with a session cookie and mobile with bearer tokens; admin endpoints require a policy. See [endpoint access](docs/sdd/architecture/security/authorization/roles-and-policies.md#endpoint-access) before touching access rules.

## Project context

- [Architecture](docs/sdd/architecture/README.md) — modules, layers, and dependency boundaries.
- [Architecture decisions (ADR)](docs/sdd/architecture/decisions/README.md) and [C4 diagrams](docs/sdd/architecture/c4/README.md) — why the system is built this way and how its parts connect. A new lasting decision (module, external system, way to run work or deploy) gets an ADR and a C4 update in the same change; the full architecture audit reports missing ones.
- [Quality targets](docs/sdd/architecture/overview/quality-targets.md) — speed, availability, freshness, RPO/RTO and cost in numbers. Every architecture review, audit, and feature design starts here: missing targets or current values are the first finding, and no speed or reliability verdict is given without them.
- [Standards](docs/sdd/standards/README.md) — stable rules for development, delivery, and verification.
- [Specifications](docs/sdd/specs/README.md) — feature requirements, decisions, tasks, and completion evidence.
- [Implemented feature map](docs/sdd/features/README.md) — navigation by module, structure, and code entry points.
- [Contracts](docs/sdd/contracts/README.md) — public API behavior and contract synchronization.
- [Operations documentation](docs/sdd/operations/README.md) — startup, configuration, migrations, seed/jobs, and diagnostics.
- [Glossary](docs/product/glossary.md) — stable business terms.

Identify the module and affected documents from the request and code; do not assume another backend project uses the same implementation. For task-specific context, navigate from the [feature map](docs/sdd/features/README.md) to the relevant feature description and code; read specifications, architecture, and standards only for the issues involved.

For a new or substantially changed feature, update the [feature map](docs/sdd/features/README.md) under the [delivery rules](docs/sdd/standards/delivery-rules.md); do not document an internal detail again if it does not affect the map.

## Context search order

1. Feature map → feature card → the code in its “Code structure” and the tests in its “Tests”.
2. Search code in `src/` and `tests/` first; search `docs/sdd/` only for the relevant section.
3. Open `docs/sdd/specs/**` only for decisions or history of that feature; do not read old task files or revisions by default. Paths in completed task files may be outdated.
4. Open `docs/legacy/` only when the user asks; it is not a current rule.

## Commands

- Focused unit test (fast, no Docker): `dotnet test tests/UnitTests/UnitTests.csproj --filter "FullyQualifiedName~<Name>"`. Feature cards give the filter.
- Integration test: the same filter on `tests/IntegrationTests/IntegrationTests.csproj`; slow, needs Docker (Testcontainers), skips without it.
- Documentation: `python3 tools/docs/check_doc_links.py` and `python3 tools/docs/check_module_maps.py` (every module, host, endpoint folder and feature card is on the [system map](docs/sdd/architecture/overview/system-map.md), [module map](docs/sdd/architecture/overview/modules.md) and [feature map](docs/sdd/features/README.md); a change that adds or renames one updates them in the same PR).
- OpenAPI contract: `dotnet build src/Bootstrapper/Host.Api/Host.Api.csproj` regenerates `docs/sdd/contracts/openapi.json`; commit it with the endpoint change.
- Do not build the whole solution or run every test project unless asked; CI does that.

A small fix that restores documented behavior needs no SDD, file-role map, or audit: fix it, run the focused test, and describe the change briefly.

## Task routing

| Task | Skill | Project rules |
| --- | --- | --- |
| Specify a new or changed feature | `$backend-feature-spec` | [Delivery rules](docs/sdd/standards/delivery-rules.md) |
| New capability outside existing modules, new module, or its DB schema | `$backend-feature-spec` | [Module rules](docs/sdd/standards/module-rules.md) |
| Implement an accepted specification | `$backend-feature-delivery` | [Delivery rules](docs/sdd/standards/delivery-rules.md) |
| Multiple agents at the user's explicit request | `$backend-feature-orchestrator` | [Specifications and dependencies](docs/sdd/specs/README.md) |
| Code audit / systemic remediation plan | `$backend-code-audit` / `$backend-remediation-plan` | [Architecture](docs/sdd/architecture/README.md) and [standards](docs/sdd/standards/README.md) |
| HTTP/API and external contracts | `$backend-api-contract-sync` | [Contracts](docs/sdd/contracts/README.md) |
| Find database tables and relationships | — | [Database map](docs/sdd/architecture/data-model/README.md): start with the tree, then read only the relevant owner's file |
| Database schema, migrations, and seed data | `$backend-database-change` | [Database rules](docs/sdd/standards/database-rules.md) and [data operations](docs/sdd/operations/data/migrations-and-seeding.md) |
| Queries and N+1, indexes and search, transactions, concurrency, how migrations reach production | — | [Database rules](docs/sdd/standards/database-rules.md): what, where, and how; every audit checks each section and its known deviations |
| Double submit, retries, outbox and webhook handlers, sync jobs (what a repeat does) | — | [Idempotency rules](docs/sdd/standards/idempotency-rules.md): decision table, `Idempotency-Key` pattern, audit of every write operation |
| Rate limits: a new public, expensive, or credential endpoint; tuning limits | — | [Rate limit rules](docs/sdd/standards/rate-limit-rules.md): decision per endpoint, policy table, configuration, edge layer |
| Verify changes / hand off for review | `$backend-change-verification` / `$backend-pr-handoff` | [Testing rules](docs/sdd/standards/testing-rules.md) |
| Differences between code and documentation | `$backend-docs-sync` | [Standards index](docs/sdd/standards/README.md) and [feature map](docs/sdd/features/README.md) |

**Rate limits:** every endpoint gets a written rate limit decision in its SDD: the global soft limit is enough for a cheap signed-in call; anonymous, upload, import/sync/report/export, credential, and message-sending endpoints name a policy with `RequireRateLimiting` ([rate limit rules](docs/sdd/standards/rate-limit-rules.md)); `RateLimitCoverageTests` enforces it.

**API contract:** `docs/sdd/contracts/openapi.json` is generated from the code on every build and is the only machine-readable contract; never write OpenAPI YAML by hand. Each endpoint carries `WithName` (unique camelCase operation id), `WithSummary`, `Produces<T>` and `ProducesProblem`; CI fails when the committed file differs from the code, and `OpenApiDocumentRulesTests` checks ids, summaries, typed reads and number types. kedr-web downloads this file from `master` (the kedr-web branch syncs it itself after the backend merges, [order](docs/sdd/contracts/README.md#changing-the-api)) and calls the API through a typed `openapi-fetch` client ([contracts](docs/sdd/contracts/README.md), [ADR-0013](docs/sdd/architecture/decisions/0013-openapi-generated-from-code.md)).

**Folder axiom:** a folder holds at most 6 code files; when it would hold more, split it by meaning into named subfolders of at least two files each. No empty folders, no wrapper folders, no new one-file folders; only generated code (EF `Migrations/`, `Generated/`) is exempt. It applies to `src/` and `tests/`, the architecture tests enforce it, and every audit checks it ([module rules](docs/sdd/standards/module-rules.md#the-folder-axiom-at-most-six-files)).

**Idempotency:** a repeated request or message must not do the work twice. Creates of orders, money, stock, numbered documents, public form submissions, offline-queue sends, and anything that sends a message outside take an `Idempotency-Key` saved in the same transaction under a unique index; a repeat returns the first result and the same key with other data answers 409; outbox handlers, scheduled tasks, syncs, and webhooks check state or dedupe before acting ([idempotency rules](docs/sdd/standards/idempotency-rules.md)).

**Database:** reads use `AsNoTracking` or a projection and page in SQL; no query or `SaveChanges` inside a loop; one `SaveChanges` per command (repositories never save on their own); search splits the input into words, each an escaped `ILIKE` on a `gin_trgm_ops`-indexed column; shared state gets a named race protection (conditional `UPDATE`, row version, or lock) and every idempotency key a unique index with a caught violation; migrations are a separate pre-deploy step and keep the previous release working ([database rules](docs/sdd/standards/database-rules.md)).

For a new feature, apply the [delivery rules](docs/sdd/standards/delivery-rules.md) and [backend code organization rules](docs/sdd/standards/backend-rules.md): plan the file-role map in the SDD first and compare it with actual code before handoff.

**Branch names:** `<type>/<short-kebab-description>` in English, 3-6 words, types `feat fix hotfix refactor perf docs test chore`, e.g. `feat/site-order-1c-link`; the same name in every repository of one feature; no `claude`, hashes, or dates. The `Branch name` workflow fails a pull request otherwise ([rules](docs/sdd/standards/delivery-rules.md#branch-names)).

**Authorship:** Commits, merge and squash commits, and pull request descriptions carry no AI attribution: no `Co-Authored-By`, no `Claude-Session`, and no "Generated with Claude Code" line. The author and committer is the repository owner only (Weretik).

**Commit and PR titles:** every commit, pull request title and squash commit follows Conventional Commits `<type>(<scope>): <description>`: English, an optional lowercase scope (module or app), the description starting with a lowercase letter, no trailing period, at most 72 characters; types `feat`, `fix`, `refactor`, `perf`, `docs`, `test`, `chore`, `ci`, `build`; for example `fix(catalog): keep the category filter after reload`. The squash merge takes the PR title as the commit message, and the `PR title` workflow checks it on every pull request.

## Process priority

The user's direct request and this repository's rules define the scope of work. For feature specification, implementation, and verification, use the relevant `backend-*` skills and project documents as the primary process. Apply additional skills only for a needed method; they do not add repeated approval, mandatory agents, a worktree, or a separate TDD cycle on top of the project process. Run checks and tests according to this repository's rules and the task requirements.

## Scope and verification

Complete all ready tasks within the agreed feature, phase, scenario, or task scope without a separate instruction for each file; stop at the scope boundary, an unresolved product decision, or a concrete blocker. The detailed procedure is in the [delivery rules](docs/sdd/standards/delivery-rules.md). Choose checks according to the [testing rules](docs/sdd/standards/testing-rules.md) and the configuration of affected projects. For new identifiers, read the [ID strategy](docs/sdd/standards/identifier-strategy.md).

Starting `Host.Api` and working with the database can change state; before doing either, check the [project procedure for migrations and seed](docs/sdd/operations/data/migrations-and-seeding.md) and the permitted environment.

Make commits, push, create a branch or PR, or take other actions that change remote state only at the user's direct request. Preserve unrelated working-tree changes. All links here point only to files in this repository; skills are available globally by name, and this repository has no local copies.
