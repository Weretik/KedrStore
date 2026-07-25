# ADR 0003: Use ASP.NET Core Identity for Authentication

## Status
Accepted

## Date
2025-06-04

## Context
KedrStore requires user registration, login, roles, and access control for both frontend users and admin panel. A flexible, extensible and secure identity management system is needed.

Alternatives considered:
- ASP.NET Core Identity
- Custom authentication from scratch
- External identity providers (Auth0, Firebase)

ASP.NET Core Identity was selected because it:

- Integrates well with EF Core and ASP.NET Core pipeline
- Provides extensible models for users, roles, and claims
- Supports two-factor auth, password recovery, email confirmation
- Is fully open-source and battle-tested in enterprise scenarios

## Decision
We will use **ASP.NET Core Identity**, with `IdentityDbContext` configured separately from application `DbContext`. Identity will be extended using custom user entities and role logic if needed.

## Consequences

### Positive
- Built-in secure handling of login, registration, and password hashing
- Ready-to-use role/claim-based authorization
- Integration with DI and middlewares

### Negative
- Adds complexity and migrations for identity schema
- Coupled with EF Core — changing ORM requires extra work
