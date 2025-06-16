# ADR 0019: Use Mapping System

## Date
2025-06-17

## Status
Accepted

## Context
Mapping is a crucial feature for transforming objects between different layers of the application, such as domain entities, DTOs, and database models. The Kedr E-Commerce Platform uses AutoMapper to simplify and centralize mapping logic, ensuring consistency and maintainability across the application.

## Decision
We decided to implement a centralized mapping system using AutoMapper to:

1. Simplify object transformations between layers.
2. Ensure consistency in mapping logic across the application.
3. Reduce boilerplate code for manual mapping.
4. Enable automatic discovery and application of mappings via the `IMapWith<T>` interface.

## Consequences
### Positive
1. Reduces boilerplate code for object transformations.
2. Improves maintainability by centralizing mapping logic.
3. Enables automatic validation of mapping configurations.
4. Simplifies integration with database queries using `ProjectTo`.

### Negative
1. Adds a dependency on AutoMapper, which must be maintained and updated.
2. Requires developers to understand and correctly implement mapping configurations.
3. May introduce runtime errors if mappings are misconfigured.

## Example
Mapping is implemented using the following components:

**IMapWith<T>**:
```csharp
public interface IMapWith<T>
{
    void Mapping(Profile profile);
}
```

**MappingProfile.cs**:
```csharp
public class MappingProfile : Profile
{
    public MappingProfile()
    {
        ApplyMappingsFromAssembly(Assembly.GetExecutingAssembly());
    }

    private void ApplyMappingsFromAssembly(Assembly assembly)
    {
        var types = assembly.GetExportedTypes()
            .Where(type => type.GetInterfaces()
                .Any(i => i.IsGenericType &&
                          i.GetGenericTypeDefinition() == typeof(IMapWith<>)))
            .ToList();

        foreach (var type in types)
        {
            var instance = Activator.CreateInstance(type);
            var methodInfo = type.GetMethod("Mapping") ??
                             type.GetInterface("IMapWith`1")?.
                                 GetMethod("Mapping");

            methodInfo?.Invoke(instance, new object[] { this });
        }
    }
}
```

**Usage in a Service**:
```csharp
public class UserService
{
    private readonly IMapper _mapper;
    private readonly IUserRepository _repository;

    public UserService(IMapper mapper, IUserRepository repository)
    {
        _mapper = mapper;
        _repository = repository;
    }

    public async Task<List<UserDto>> GetAllUsersAsync(CancellationToken cancellationToken)
    {
        var users = _repository.GetAll();
        return await users.ProjectToListAsync<User, UserDto>(_mapper.ConfigurationProvider, cancellationToken);
    }
}
```
