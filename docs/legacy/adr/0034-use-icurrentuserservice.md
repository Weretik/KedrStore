# ADR 0034: Use ICurrentUserService

## Date
2025-06-17

## Status
Accepted

## Context
Accessing information about the current user is a common requirement in modern applications, especially for implementing authentication, authorization, and personalization features. `ICurrentUserService` provides a clean abstraction for accessing user-related data, such as user ID, roles, claims, and permissions, without coupling the application layer to specific authentication mechanisms.

## Decision
We decided to use `ICurrentUserService` in the project to:

1. Provide a centralized and consistent way to access information about the current user.
2. Decouple the application layer from specific authentication mechanisms.
3. Simplify the implementation of authentication, authorization, and personalization features.
4. Align with best practices for clean and maintainable architecture.

## Consequences
### Positive
1. Improves maintainability by centralizing user-related logic.
2. Decouples the application layer from authentication mechanisms.
3. Simplifies testing by allowing mock implementations of `ICurrentUserService`.
4. Enhances flexibility by supporting different authentication providers.

### Negative
1. Adds complexity by introducing an additional abstraction layer.
2. Requires careful implementation to ensure security and consistency.

## Example
`ICurrentUserService` is implemented as follows:

**ICurrentUserService.cs**:
```csharp
public interface ICurrentUserService
{
    string UserId { get; }
    bool IsAuthenticated { get; }
    string UserName { get; }
    string Email { get; }
    IEnumerable<string> Roles { get; }
    IEnumerable<Claim> Claims { get; }

    bool IsInRole(string role);
    bool HasPermission(string permission);
    string GetClaimValue(string claimType);
}
```

**Usage in Application Layer**:
```csharp
public class ProductService
{
    private readonly ICurrentUserService _currentUserService;

    public ProductService(ICurrentUserService currentUserService)
    {
        _currentUserService = currentUserService;
    }

    public bool CanUserManageProducts()
    {
        return _currentUserService.HasPermission("Products.Manage");
    }
}
```
