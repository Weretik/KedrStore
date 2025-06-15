# Infrastructure Layer

This document describes the structure and implementation principles of the **Infrastructure Layer** in the `KedrStore` project, following **Clean Architecture** and **DDD** best practices.

---

## 🎯 Purpose of the `Infrastructure` Layer

The `Infrastructure` layer provides implementations for technical concerns and external integrations, such as:
- Data persistence (e.g., Entity Framework Core, PostgreSQL)
- External services (e.g., email, payment gateways, file storage)
- Security and authentication providers
- Background jobs and messaging
- Logging and monitoring

It contains code that interacts with frameworks, databases, and external APIs, but **does not contain business logic**.

---

## 📁 Structure

```
Infrastructure/
├── Catalog/                # Data access for Catalog module
│   ├── Ef/                 # EF Core DbContext, configurations
│   └── Repositories/       # Repository implementations
├── Identity/               # Identity and authentication providers
├── Security/               # Security, authorization, JWT, etc.
├── Shared/                 # Shared infrastructure (e.g., email, file storage)
├── Extensions/             # Extension methods for DI, configuration
├── InfrastructureAssemblyMarker.cs
```

---

## 🧩 Key Concepts

### 📌 Repository Implementations
- Implement domain repository interfaces (e.g., `IProductRepository`)
- Use EF Core or other ORMs for data access
- Map domain entities to persistence models

### 📌 External Services
- Integrate with third-party APIs (e.g., email, SMS, payment)
- Implement interfaces defined in Application/Domain layers

### 📌 Security & Identity
- Configure authentication and authorization providers
- Implement user/role management, JWT, OAuth, etc.

### 📌 Dependency Injection
- Register infrastructure services in DI container
- Use extension methods for clean startup configuration

---

## 🛠 Design Rules

- ❌ No business logic in infrastructure
- ✅ Only technical concerns and integrations
- Infrastructure depends on Domain & Application, but not vice versa
- Use configuration and environment variables for secrets
- Keep external dependencies isolated and replaceable

---

## 🧪 Testing

- Infrastructure code is tested via integration tests
- Use test containers or in-memory providers for DB and services
- Mock external APIs in tests

---

## 🧱 Extension Guide

To add a new infrastructure feature:

1. Implement the required interface (from Domain/Application)
2. Register the implementation in DI via extension method
3. Add configuration to `appsettings.json` if needed
4. Cover with integration tests

---

## 📎 Examples

### Repository Implementation
```csharp
public class ProductRepository : IProductRepository
{
    private readonly AppDbContext _context;
    public ProductRepository(AppDbContext context)
    {
        _context = context;
    }
    public async Task<Product?> GetByIdAsync(ProductId id)
        => await _context.Products.FindAsync(id.Value);
    // ...other methods...
}
```

### External Service Implementation
```csharp
public class EmailSender : IEmailSender
{
    public Task SendAsync(string to, string subject, string body)
    {
        // Integrate with SMTP or third-party email API
    }
}
```

### DI Registration Extension
```csharp
public static class InfrastructureServiceCollectionExtensions
{
    public static IServiceCollection AddInfrastructure(this IServiceCollection services, IConfiguration config)
    {
        services.AddScoped<IProductRepository, ProductRepository>();
        // ...register other services...
        return services;
    }
}
```

---

## 📝 Naming & Organization Tips
- Use clear, technology-specific names for infrastructure classes (e.g., `EfProductRepository`, `SmtpEmailSender`)
- Place configuration and DI extensions in the `Extensions` folder
- Keep infrastructure code isolated from domain logic
- Organize by business subdomain and technical concern

---

This documentation can be extended to fit your team's processes and standards. If you need examples for other patterns or modules, let us know!

