# ADR 0023: Use Dependency Injection via Extensions

## Date
2025-06-17

## Status
Accepted

## Context
Dependency Injection (DI) is a fundamental design pattern for managing dependencies in modern applications. Using extension methods to configure DI ensures a clean and modular approach, centralizing the registration logic and improving maintainability. In the Kedr E-Commerce Platform, DI via extensions simplifies the setup of services and promotes consistency across layers.

## Decision
We decided to use extension methods for configuring Dependency Injection in the project to:

1. Centralize DI configuration logic in dedicated extension methods.
2. Improve modularity and maintainability by grouping related service registrations.
3. Ensure consistency in DI setup across different layers (Application, Domain, Infrastructure).
4. Align with best practices for modern .NET applications.

## Consequences
### Positive
1. Simplifies DI configuration by centralizing logic in extension methods.
2. Improves modularity and maintainability of the codebase.
3. Promotes consistency in DI setup across layers.
4. Makes it easier to understand and modify DI configurations.

### Negative
1. Adds a layer of abstraction, which may confuse developers unfamiliar with the approach.
2. Requires careful management of extension methods to avoid duplication or conflicts.
3. May lead to subtle bugs if extension methods are not properly tested.

## Example
DI via extensions is implemented as follows:

**ApplicationServiceCollection.cs**:
```csharp
public static class ApplicationServiceCollection
{
    public static IServiceCollection AddApplicationServices(this IServiceCollection services)
    {
        services.AddMediatR(cfg => {
            cfg.RegisterServicesFromAssembly(Assembly.GetExecutingAssembly());
            cfg.AddBehavior(typeof(IPipelineBehavior<,>), typeof(DomainEventDispatcherBehavior<,>));
        });

        services.AddScoped<IDomainEventDispatcher, DomainEventDispatcher>();
        services.AddDomainEventHandlers(Assembly.GetExecutingAssembly());
        services.AddAutoMapper(Assembly.GetExecutingAssembly());

        return services;
    }

    public static IServiceCollection AddDomainEventHandlers(this IServiceCollection services, Assembly assembly)
    {
        var handlerTypes = assembly.GetTypes()
            .Where(t => t.GetInterfaces()
                .Any(i => i.IsGenericType &&
                          i.GetGenericTypeDefinition() == typeof(IDomainEventHandler<>)))
            .ToList();

        foreach (var handlerType in handlerTypes)
        {
            var interfaceType = handlerType.GetInterfaces()
                .First(i => i.IsGenericType &&
                            i.GetGenericTypeDefinition() == typeof(IDomainEventHandler<>));

            services.AddScoped(interfaceType, handlerType);
        }

        return services;
    }
}
```
