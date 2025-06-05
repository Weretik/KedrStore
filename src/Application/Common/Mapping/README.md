# Mapping System

## Overview

This module provides a centralized approach to object mapping across different application layers using AutoMapper.

## Components

### IMapWith<T>

An interface that should be implemented by all classes requiring mapping to or from another type.

```csharp
public class UserDto : IMapWith<User>
{
    public int Id { get; set; }
    public string Name { get; set; }

    public void Mapping(Profile profile)
    {
        profile.CreateMap<User, UserDto>();
    }
}
```

### MappingProfile

Automatically discovers and applies all mappings defined via the `IMapWith<T>` interface.

### MappingExtensions

Provides convenient extension methods for mapping operations:

- `ProjectTo<TSource, TDestination>` – projects a query to another type
- `MapList<TSource, TDestination>` – maps a list of objects
- `ProjectToListAsync<TSource, TDestination>` – asynchronous projection and retrieval of a list

### EntityMappingExtensions

Includes additional methods for working with entities:

- `MapTo<TSource, TDestination>` – updates an existing object
- `SyncList<TSource, TDestination, TKey>` – synchronizes object lists

## Usage

### DI Registration

```csharp
services.AddAutoMapperProfiles(typeof(IMapWith<>).Assembly);
```

### Example Usage in a Service

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

## Usage Tips

1. Always define mappings in DTO classes via `IMapWith<T>`
2. Use `ProjectTo` instead of `Select(...Map...)` for database queries
3. Validate mapping configuration on application startup using `MappingHelper.ValidateMapperConfiguration()`
4. Use `mapper.MapTo(source, destination)` when updating existing entities
