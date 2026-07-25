# Application Layer Interfaces in KedrStore

## Overview

Interfaces in the Application layer define contracts for interaction with external systems and services, enabling dependency inversion. These interfaces are implemented in the Infrastructure layer, allowing the Application layer to remain independent of specific implementations.

## Dependency Inversion Principle (DIP)

Use of interfaces in the Application layer is based on the Dependency Inversion Principle:

1. **High-level modules** (Application) should not depend on low-level modules (Infrastructure)
2. **Both modules** should depend on abstractions
3. **Abstractions** should not depend on details
4. **Details** should depend on abstractions

## Key Interfaces

- `IApplicationDbContext` – EF Core database access abstraction
- `IUnitOfWork` – transaction management
- `ICurrentUserService` – current user context
- `IDateTimeProvider` – access to date/time
- `IEmailService` – email delivery
- `IFileStorageService` – file storage
- `ICacheService` – caching logic
- `IBackgroundJobService` – background job management

## Structure and Implementation

These interfaces allow the Application layer to remain decoupled and testable.

### Example: IApplicationDbContext

Defines EF Core access abstraction:
```csharp
public interface IApplicationDbContext
{
    DbSet<Product> Products { get; }
    Task<int> SaveChangesAsync(CancellationToken cancellationToken = default);
    ChangeTracker ChangeTracker { get; }
}
```

### Example: IUnitOfWork

```csharp
public interface IUnitOfWork
{
    Task<IDbContextTransaction> BeginTransactionAsync(CancellationToken cancellationToken = default);
    Task SaveChangesAsync(CancellationToken cancellationToken = default);
    Task CommitTransactionAsync(IDbContextTransaction transaction, CancellationToken cancellationToken = default);
    Task RollbackTransactionAsync(IDbContextTransaction transaction, CancellationToken cancellationToken = default);
}
```

### Example: ICurrentUserService

```csharp
public interface ICurrentUserService
{
    string? UserId { get; }
    string? Email { get; }
    string? UserName { get; }
    IEnumerable<string> Roles { get; }
    bool IsAuthenticated { get; }
    bool IsInRole(string role);
}
```

### Example: IDateTimeProvider

```csharp
public interface IDateTimeProvider
{
    DateTime UtcNow { get; }
    DateOnly UtcToday { get; }
    DateTime LocalNow { get; }
    DateOnly LocalToday { get; }
}
```

### Example: IEmailService

```csharp
public interface IEmailService
{
    Task SendEmailAsync(string to, string subject, string body, CancellationToken cancellationToken = default);
    Task SendEmailWithAttachmentsAsync(string to, string subject, string body, IEnumerable<EmailAttachment> attachments, CancellationToken cancellationToken = default);
    Task SendEmailToMultipleRecipientsAsync(IEnumerable<string> to, string subject, string body, CancellationToken cancellationToken = default);
}
```

### Example: IFileStorageService

```csharp
public interface IFileStorageService
{
    Task<string> SaveFileAsync(string fileName, byte[] fileContent, string contentType, CancellationToken cancellationToken = default);
    Task<FileDto> GetFileAsync(string fileId, CancellationToken cancellationToken = default);
    Task DeleteFileAsync(string fileId, CancellationToken cancellationToken = default);
    string GetFileUrl(string fileId);
}
```

### Example: ICacheService

```csharp
public interface ICacheService
{
    Task<T?> GetAsync<T>(string key, CancellationToken cancellationToken = default) where T : class;
    Task SetAsync<T>(string key, T value, TimeSpan? expirationTime = null, CancellationToken cancellationToken = default) where T : class;
    Task RemoveAsync(string key, CancellationToken cancellationToken = default);
    Task<bool> ExistsAsync(string key, CancellationToken cancellationToken = default);
}
```

### Example: IBackgroundJobService

```csharp
public interface IBackgroundJobService
{
    Task<string> EnqueueAsync<T>(T job, CancellationToken cancellationToken = default) where T : class;
    Task<string> ScheduleAsync<T>(T job, TimeSpan delay, CancellationToken cancellationToken = default) where T : class;
    Task<string> RecurringAsync<T>(T job, string cronExpression, CancellationToken cancellationToken = default) where T : class;
    Task CancelAsync(string jobId, CancellationToken cancellationToken = default);
}
```

## Implementation in Infrastructure

Interfaces are implemented in the Infrastructure layer, registered in the `Startup.cs` or equivalent:

```csharp
services.AddScoped<IApplicationDbContext>(provider => provider.GetRequiredService<ApplicationDbContext>());
services.AddScoped<IUnitOfWork, UnitOfWork>();
services.AddScoped<ICurrentUserService, CurrentUserService>();
services.AddSingleton<IDateTimeProvider, DateTimeProvider>();
services.AddScoped<IEmailService, EmailService>();
services.AddScoped<IFileStorageService, FileStorageService>();
services.AddSingleton<ICacheService, CacheService>();
services.AddSingleton<IBackgroundJobService, BackgroundJobService>();
```

## Testing

Interfaces facilitate mocking:

```csharp
var mockDbContext = new Mock<IApplicationDbContext>();
var mockUserService = new Mock<ICurrentUserService>();
var mockDateTime = new Mock<IDateTimeProvider>();
```

## Conclusion

Interfaces in the Application layer are essential for maintaining a clean architecture. They enable:

- Decoupling from implementation details
- Easy unit testing
- Dependency injection
- Long-term maintainability
