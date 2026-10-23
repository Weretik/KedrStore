# Identity, authentication, and authorization

This architecture section covers accounts, authentication, authorization, and access-control planning. It is separate from `platform`: the platform describes technologies, while this section describes how the application uses them.

```text
security/
├── authentication/  login, bearer tokens, refresh, logout, and CSRF
├── authorization/   fallback protection, roles, and named policies
├── users/           initial users, import, and registration design
└── planning/        SDD checklist for a feature with access control
```

## Navigate by task

- Session, token, or refresh issue: [authentication](authentication/README.md)
- Endpoint protection, authorization policy, or role: [authorization](authorization/README.md)
- User creation, registration, or invitations: [users](users/README.md)
- Planning a feature with permissions: [checklist](planning/access-control-checklist.md)

## Responsibilities

```text
Identity.Api             HTTP session and future Identity endpoints
        |
Identity.Application     commands/queries, validators, and service contracts
        |
Identity.Infrastructure  ASP.NET Core Identity, tokens, sessions, AppIdentityDbContext
        |
Identity.Domain          role names

Host.Api                 authentication middleware and authorization policies
```

Identity owns accounts and credentials. A business module may protect its endpoint through an authorization policy, but it does not create users, check passwords, or call `UserManager<AppUser>` directly.
