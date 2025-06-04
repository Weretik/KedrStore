# ADR 0011: Separate IdentityDbContext and AppDbContext

## Status
Accepted

## Date
2025-06-04

## Context
KedrStore uses ASP.NET Core Identity for authentication and user management.  
The application also uses its own domain models and EF Core mappings for catalog, orders, CRM, etc.

Mixing identity-related entities (e.g. `IdentityUser`, `IdentityRole`) into the main `AppDbContext` would violate separation of concerns, reduce maintainability, and increase coupling.

## Decision
We will maintain **two separate EF Core DbContexts**:
- `IdentityDbContext` — for users, roles, claims, tokens
- `AppDbContext` — for all domain-specific entities

Each context will:
- Have its own configuration and migrations
- Be injected separately via DI
- Be used in different layers (Identity in Infrastructure/Auth; AppDbContext in Infrastructure/Persistence)

This enables clear ownership of responsibility, modular migrations, and future flexibility (e.g., replacing Identity or using another auth system).

## Consequences

### Positive
- Clean separation of identity and application data
- Independent migrations for each context
- Easier testing and management of schemas
- Aligns with Clean Architecture principles

### Negative
- Requires dual setup in DI and migrations
- Slightly more complex initial configuration
