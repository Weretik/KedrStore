# ADR: Centralize route definitions in a single Routing.cs

- Status: Accepted
- Date: 2025-08-08
- Decision owner: Team
- Related area: Navigation, UI, Routing

## Context

We have multiple UI components and services that need to navigate between pages and build links. Previously, routes were referenced as ad‑hoc string literals across the codebase, which led to duplication, typos, and made refactoring brittle. We want a single source of truth for client-facing routes that provides compile-time safety, improves discoverability, and simplifies changes.

## Decision

We will centralize all client-facing route constants in a single static class defined in:
- src/Presentation/Presentation.Shared/Common/Routing.cs

All components, pages, and services must reference routes through this class instead of hard-coded strings.

Key points:
- Use public const string fields grouped by area (e.g., Home, Catalog, Info) for commonly used routes.
- Prefer meaningful names that reflect domain intent (e.g., CatalogProducts, InfoShipping).
- Include query-parameter variants only when they are stable and part of user-facing navigation (e.g., tab selection).
- Keep the file platform-agnostic within the UI layer (no service or environment-specific logic).
- When adding a new page, add its route constant in this file in the appropriate group.

Example usage (illustrative):
- Use Routing.Home instead of "/".
- Use Routing.CatalogProducts instead of "/catalog/hardware".

## Status

Accepted.

## Consequences

Positive:
- Single source of truth for routes; easier and safer refactoring.
- Reduced risk of typos and broken links thanks to compile-time checks.
- Improved discoverability for developers (one place to look for routes).
- Consistent naming and navigation patterns across the codebase.

Negative:
- The file can grow and become crowded over time.
- Higher chance of merge conflicts when many contributors touch the same file.

Mitigations:
- Keep routes grouped by domain/feature with clear section headers.
- Curate names during code review; avoid one-off route constants scattered elsewhere.
- Periodically prune unused routes and reorganize sections when needed.

## Alternatives considered

1) Keep using ad-hoc string literals
- Rejected due to duplication, error-proneness, and poor refactorability.

2) Generate route constants from attributes or a build step
- Rejected for now due to added complexity and tooling overhead; may reconsider if route surface grows significantly.

3) Store routes in configuration (JSON/resource files)
- Rejected because routes are compile-time concerns; config adds indirection without clear benefit.

## Decision drivers

- Developer experience and productivity.
- Consistency and maintainability.
- Compile-time safety during refactors.

## Scope

- Applies to all UI-related route usages (navigation, link construction, redirects) in the Presentation layer.
- Does not cover backend endpoint URLs or API routes.

## Follow-up

- Enforce this convention in code reviews.
- Consider lightweight tooling (linters/analyzers) later to flag hard-coded route literals in UI code.
