# ADR 0048: Use IPermissionService

## Date
2025-06-17

## Status
Accepted

## Context
`IPermissionService` is an interface designed to manage and validate user permissions within the application. It provides a clean abstraction for checking whether a user has the required permissions to perform specific actions. In the Kedr E-Commerce Platform, `IPermissionService` is used to centralize permission validation logic, ensuring consistency and maintainability across the application.

## Decision
We decided to use `IPermissionService` in the project to:

1. Centralize permission validation logic for better maintainability.
2. Provide a reusable and consistent way to check user permissions.
3. Simplify the implementation of authorization features across the application.
4. Align with best practices for secure and scalable access control.

## Consequences
### Positive
1. Improves maintainability by centralizing permission validation logic.
2. Ensures consistency in permission checks across the application.
3. Simplifies debugging and testing of authorization features.
4. Enhances flexibility by supporting different authentication providers.

### Negative
1. Adds complexity by introducing an additional abstraction layer.
2. Requires careful implementation to ensure security and reliability.

## Example
`IPermissionService` is implemented as follows:

**IPermissionService.cs**:
```csharp
namespace Application.Common.Abstractions.Security;

public interface IPermissionService
{
    Task<bool> HasPermissionAsync(int userId, string permission, CancellationToken cancellationToken = default);
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
        return await _permissionService.HasPermissionAsync(userId, "Products.Manage", cancellationToken);
    }
}
```
