# Identity — access and sessions: data

**Source:** [Identity snapshot](../../../../src/Identity/Identity.Infrastructure/Migrations/AppIdentityDbContextModelSnapshot.cs) and [configurations](../../../../src/Identity/Identity.Infrastructure/Configuration/), checked 2026-10-21. [Back to database map](README.md) · [session feature](../../features/identity/session.md).

| Table | PK | Purpose |
| --- | --- | --- |
| `AspNetUsers` | `Id` | Users and credentials; `FullName`, `AvatarPath` (CDN file key, nullable). |
| `AspNetRoles` | `Id` | Roles and access levels. |
| `AspNetUserRoles` | `UserId`, `RoleId` | User role assignments. |
| `AspNetUserClaims` | `Id` | User claims (`UserId` FK). |
| `AspNetRoleClaims` | `Id` | Role claims (`RoleId` FK). |
| `AspNetUserLogins` | `LoginProvider`, `ProviderKey` | External logins (`UserId` FK). |
| `AspNetUserTokens` | `UserId`, `LoginProvider`, `Name` | Tokens (`UserId` FK). |
| `AppRefreshSessions` | `Id` | Refresh sessions (`UserId` FK); `xmin` guards rotation. |
| `DataProtectionKeys` | `Id` | ASP.NET Core Data Protection key ring for tokens and cookies. |

```mermaid
erDiagram
    AspNetUsers ||--o{ AspNetUserRoles : "UserId FK"
    AspNetRoles ||--o{ AspNetUserRoles : "RoleId FK"
    AspNetUsers ||--o{ AspNetUserClaims : "UserId FK"
    AspNetRoles ||--o{ AspNetRoleClaims : "RoleId FK"
    AspNetUsers ||--o{ AspNetUserLogins : "UserId FK"
    AspNetUsers ||--o{ AspNetUserTokens : "UserId FK"
    AspNetUsers ||--o{ AppRefreshSessions : "UserId FK"
```
