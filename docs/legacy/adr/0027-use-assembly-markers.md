# ADR 0027: Use Assembly Markers

## Date
2025-06-17

## Status
Accepted

## Context
Assembly markers are a design pattern used to identify and reference specific assemblies within a project. They provide a simple and effective way to centralize assembly-related metadata and simplify operations such as dependency injection, MediatR registration, and AutoMapper configuration. In the Kedr E-Commerce Platform, assembly markers are used to define the boundaries of layers (Domain, Application, Infrastructure, Presentation).

## Decision
We decided to use assembly markers in the project to:

1. Simplify the identification and referencing of assemblies.
2. Centralize assembly-related metadata for better maintainability.
3. Enable consistent configuration of DI, MediatR, and AutoMapper.
4. Align with Clean Architecture principles by clearly defining layer boundaries.

## Consequences
### Positive
1. Simplifies configuration of DI, MediatR, and AutoMapper.
2. Improves maintainability by centralizing assembly-related metadata.
3. Promotes consistency in layer boundaries and assembly management.
4. Reduces boilerplate code for assembly referencing.

### Negative
1. Adds a small amount of overhead by introducing marker classes.
2. Requires developers to understand the purpose and usage of assembly markers.

## Example
Assembly markers are implemented as empty classes in each layer. For example:

**DomainAssemblyMarker.cs**:
```csharp
namespace Domain;

public sealed class DomainAssemblyMarker
{
}
```

**ApplicationAssemblyMarker.cs**:
```csharp
namespace Application;

public sealed class ApplicationAssemblyMarker
{
}
```

**Usage in DI Configuration**:
```csharp
services.AddMediatR(cfg => {
    cfg.RegisterServicesFromAssembly(typeof(ApplicationAssemblyMarker).Assembly);
});

services.AddAutoMapper(typeof(ApplicationAssemblyMarker).Assembly);
```
