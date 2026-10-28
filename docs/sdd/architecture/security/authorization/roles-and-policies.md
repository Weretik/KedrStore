# Authorization

## Default rule

`Host.Api` configures a fallback authorization policy that requires an authenticated user. Therefore, an endpoint is protected unless it explicitly declares `[AllowAnonymous]`. Anonymous access must be explicitly described in the API contract and verified as part of the feature.

Authentication answers **who is calling**; authorization answers **whether that caller may perform the operation**.

## Current state: authorization not yet enforced

**Temporary, checked 2026-10-27.** User authorization is not rolled out yet, so the Admin and storefront controllers below are deliberately marked `[AllowAnonymous]`. The fallback policy above is configured but does not protect them. Until the rollout, a new endpoint in these controllers follows the same temporary rule; do not treat this as the target design.

| Controller | Routes | Now | Target after rollout |
| --- | --- | --- | --- |
| `AdminOrderWorkspaceController` | `/api/admin/order-workspace` | anonymous (class) | `CanManageOrders` |
| `AdminOrdersController` | `/api/admin/orders` | anonymous (per action) | `CanManageOrders` |
| `AdminCustomersController` | `/api/admin/customers` | anonymous (class) | `CanManageOrders` or a customer policy |
| `AdminProductsController` | `/api/admin/products` | anonymous (class) | `CanManageProducts` |
| `AdminCategoriesController` | `/api/categories` | anonymous (class) | `CanManageProducts` |
| `CategoriesController`, `ProductsController` | `/api/catalog/{lang}` | anonymous (class) | stays public |
| `OrderController` | `POST /api/orders` (quick order) | anonymous | stays public, rate-limited |
| `AuthSessionController` | login, refresh | anonymous | stays public, rate-limited |
| `CatalogController` (Sales) | `/api/sales/{lang}/catalog` | `[Authorize]` | already protected |

**When authorization is introduced:** remove `[AllowAnonymous]` from every row whose target is a policy, apply the named policy, add `401`/`403` to the affected OpenAPI contracts and synchronize them to kedr-web, add API tests for denied and allowed calls, and delete this section. Use the [access-control checklist](../planning/access-control-checklist.md).

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
