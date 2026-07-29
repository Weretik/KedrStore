# Authorization

## Default rule

`Host.Api` configures a fallback authorization policy requiring an authenticated user. An endpoint is therefore protected unless it deliberately declares `[AllowAnonymous]`. Anonymous access must be explicit in the API contract and reviewed as part of the feature.

Authentication answers **who is calling**. Authorization answers **whether that caller may perform this operation**.

## Roles

Role constants live in `Identity.Domain/Authorization/RoleNames.cs`.

| Role | Current meaning | Seeded at startup |
| --- | --- | --- |
| `Admin` | System administration | yes |
| `Manager` | Content-management access | yes |
| `User` | Regular authenticated user | yes |
| `Guest` | Defined role constant | no |

The startup `RoleSeeder` currently creates only Admin, Manager and User. Do not assign `Guest` until its seeding and business meaning are deliberately added.

## Policies

Policy names live in `Identity.Application/Security/Policies/PolicyNames.cs`; their role requirements are composed in `Host.Api` in `AuthorizationPoliciesRegistrationExtensions`.

| Policy | Permitted roles |
| --- | --- |
| `RequireAdminRole` / `CanManageUsers` | Admin |
| `RequireManagerRole` / `CanManageProducts` / `CanManageOrders` | Manager, Admin |
| `CatalogRead` / `OrderCreate` | User, Manager, Admin |
| `RequireTenantAccess` | Defined constant; no policy is currently registered |

Do not use `RequireTenantAccess` on an endpoint until the Host registration and its semantics exist.

## Protecting an endpoint

Prefer an existing named policy that expresses the operation:

```csharp
[HttpPut("{id:guid}")]
[Authorize(Policy = PolicyNames.CanManageProducts)]
public async Task<IActionResult> Update(Guid id, ...)
```

The controller only declares access. The command/query still goes through `ISender`; business invariants stay in Domain/Application. Do not use a role check inside a controller as a substitute for a policy.

For an endpoint that must be public, use `[AllowAnonymous]` and record why in the feature contract. This overrides the fallback policy.

## Current-user and permissions infrastructure

BuildingBlocks Infrastructure provides request-user and permission abstractions such as `HttpContextCurrentUserService` and `ClaimPermissionService`. Use the existing application-facing abstraction where a use case needs the current identity. Do not inject `HttpContext`, `UserManager<AppUser>` or `RoleManager<AppRole>` into Domain or another module's Application layer.

## Adding a policy or role

1. First check whether an existing policy already captures the business operation.
2. If not, define the policy name in Identity Application and its role requirement in Host.Api's authorization registration.
3. If a new role is necessary, add its constant in Identity Domain and make startup seeding intentional. Define which existing and future users receive it.
4. Protect the endpoint with the new policy, update the API contract and manually test forbidden as well as permitted requests.

Adding a role is an authorization change, not merely a string change: it affects bootstrap, user assignment, documentation and rollout.
