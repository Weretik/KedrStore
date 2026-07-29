# Identity, authentication and authorization

This is the architecture area for user identity, authentication, authorization and access-control planning. It is deliberately outside `platform`: platform describes technologies; this section describes how the application uses them.

```text
security/
├── authentication/  login, bearer tokens, refresh, logout and CSRF
├── authorization/   fallback protection, roles and named policies
├── users/           bootstrap users, import provisioning and registration design
└── planning/        SDD checklist for a feature with access control
```

## Read by task

- A session, token or refresh issue: [authentication](authentication/README.md)
- Protecting an endpoint, adding a policy or role: [authorization](authorization/README.md)
- Creating users, registration or invitation: [users](users/README.md)
- Planning a feature with permissions: [planning checklist](planning/access-control-checklist.md)

## Ownership

```text
Identity.Api             HTTP session and future identity endpoints
        |
Identity.Application     commands/queries, validators and service contracts
        |
Identity.Infrastructure  ASP.NET Core Identity, tokens, sessions, AppIdentityDbContext
        |
Identity.Domain          role names

Host.Api                 authentication middleware and authorization policies
```

Identity owns accounts and credentials. A business module may protect its endpoint with a policy, but it must not create users, validate passwords or access `UserManager<AppUser>` directly.
