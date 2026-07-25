# ADR 0029: Use Constants and Permission System

## Date
2025-06-17

## Status
Accepted

## Context
Managing roles, permissions, policies, and claims is a critical aspect of application security and access control. Using constants for these elements ensures consistency, maintainability, and ease of use across the application. Additionally, implementing a Permission system provides a centralized way to manage and validate user access to specific resources and actions.

## Decision
We decided to use constants and a Permission system in the project to:

1. Centralize definitions of roles, permissions, policies, and claims.
2. Ensure consistency and maintainability in access control logic.
3. Simplify validation of user permissions through a dedicated service.
4. Align with best practices for secure and scalable access control.

## Consequences
### Positive
1. Improves consistency by centralizing access control definitions.
2. Simplifies validation of permissions through a dedicated service.
3. Enhances maintainability by using constants for roles, permissions, policies, and claims.
4. Promotes scalability by enabling fine-grained access control.

### Negative
1. Adds complexity by introducing additional layers for access control.
2. Requires careful management of constants to avoid duplication or conflicts.
3. May lead to performance issues if permission checks are not optimized.

## Example
Constants and the Permission system are implemented as follows:

**AppRoles.cs**:
```csharp
public static class AppRoles
{
    public const string Admin = "Admin";
    public const string User = "User";
    public const string Manager = "Manager";
    public const string Guest = "Guest";

    public static readonly IReadOnlyList<string> All = new[]
    {
        Admin,
        Manager,
        User,
        Guest
    };
}
```

**AppPermissions.cs**:
```csharp
public static class AppPermissions
{
    public const string UsersRead = "Users.Read";
    public const string UsersCreate = "Users.Create";
    public const string UsersUpdate = "Users.Update";
    public const string UsersDelete = "Users.Delete";

    public const string ProductsRead = "Products.Read";
    public const string ProductsCreate = "Products.Create";
    public const string ProductsUpdate = "Products.Update";
    public const string ProductsDelete = "Products.Delete";
}
```

**FakePermissionService.cs**:
```csharp
public class FakePermissionService : IPermissionService
{
    public Task<bool> HasPermissionAsync(int userId, string permission, CancellationToken cancellationToken = default)
    {
        // Placeholder implementation
        return Task.FromResult(true);
    }
}
```

**Usage in Application Layer**:
```csharp
public class ProductService
{
    private readonly IPermissionService _permissionService;

    public ProductService(IPermissionService permissionService)
    {
        _permissionService = permissionService;
    }

    public async Task<bool> CanUserManageProductsAsync(int userId, CancellationToken cancellationToken)
    {
        return await _permissionService.HasPermissionAsync(userId, AppPermissions.ProductsUpdate, cancellationToken);
    }
}
```
