# Authorization

## Default rule

`Host.Api` configures a fallback authorization policy that requires an authenticated user. Therefore, an endpoint is protected unless it explicitly declares `[AllowAnonymous]`. Anonymous access must be explicitly described in the API contract and verified as part of the feature.

Authentication answers **who is calling**; authorization answers **whether that caller may perform the operation**.

## Endpoint access

Checked 2026-10-28, after the [authentication rollout](../../../specs/identity/001-authentication-rollout/README.md).

| Controller | Routes | Access |
| --- | --- | --- |
| `AdminOrderWorkspaceController` | `/api/admin/order-workspace` | `CanManageOrders` |
| `AdminOrdersController` | `/api/admin/orders` | `CanManageOrders` |
| `AdminCustomersController` | `/api/admin/customers` | `CanManageOrders` |
| `AdminProductsController` | `/api/admin/products` | `CanManageProducts` |
| `AdminCategoriesController` | `/api/categories` | `CanManageProducts` |
| `AdminUsersController` | `/api/admin/users` | `CanManageUsers` |
| `CatalogController` (Sales) | `/api/sales/{lang}/catalog` | any signed-in user |
| `CategoriesController`, `ProductsController` | `/api/catalog/{lang}` | public |
| `OrderController` | `POST /api/orders` (quick order) | public, rate-limited |
| `CookieAuthController`, `MobileAuthController`, `AuthSessionController` | login, refresh | public, rate-limited; logout and `me` need a session |

A new endpoint in an admin controller inherits the controller's policy. A public endpoint needs `[AllowAnonymous]` and a reason in its contract.

## Roles

Role constants are defined in `Identity.Domain/Authorization/RoleNames.cs`.

| Role | Current meaning | Created by `Host.Jobs --job=migrate` |
| --- | --- | --- |
| `Admin` | System administration | yes |
| `Manager` | Content management | yes |
| `User` | Regular authenticated user | yes |
| `Guest` | Declared role constant | no |

`RoleSeeder`, run by the separate migration job, creates only Admin, Manager, and User. Do not assign `Guest` until its provisioning and business meaning are defined.

## Access policies

Policy names are defined in `Identity.Application/Security/Policies/PolicyNames.cs`; role requirements are configured by `Host.Api` in `AuthorizationPoliciesRegistrationExtensions`.

| Policy | Allowed roles |
| --- | --- |
| `RequireAdminRole` / `CanManageUsers` | Admin |
| `RequireManagerRole` / `CanManageProducts` / `CanManageOrders` | Manager, Admin |
| `CatalogRead` / `OrderCreate` | User, Manager, Admin |
| `RequireTenantAccess` | The constant exists, but the policy is not currently registered |

Do not use `RequireTenantAccess` on an endpoint until it is registered in the host and its semantics are defined.

## Protecting endpoints

Prefer an existing named policy that describes the operation:

```csharp
[HttpPut("{id:guid}")]
[Authorize(Policy = PolicyNames.CanManageProducts)]
public async Task<IActionResult> Update(Guid id, ...)
```

The controller only declares access. Commands and queries still go through `ISender`; business invariants remain in Domain/Application. Do not replace a policy with a role check inside the controller.

For a public endpoint, use `[AllowAnonymous]` and record the reason in the feature contract. This overrides the fallback authorization policy.

## Current-user and permission infrastructure

BuildingBlocks Infrastructure provides request-user and permission abstractions, including `HttpContextCurrentUserService` and `ClaimPermissionService`. If a use case needs current-user data, use the existing Application abstraction. Do not inject `HttpContext`, `UserManager<AppUser>`, or `RoleManager<AppRole>` into Domain or another module's Application.

## Adding a policy or role

1. Check whether an existing policy already describes the business operation.
2. If not, define the policy name in Identity Application and the role requirement in Host.Api authorization registration.
3. If a new role is needed, add a constant in Identity Domain and plan its creation through the separate migration job. Define which existing and future users receive it.
4. Protect the endpoint with the new rule, update the API contract, and manually verify both denied and allowed requests.

Adding a role changes authorization, not just a string: it affects initial provisioning, user assignment, documentation, and deployment order.
