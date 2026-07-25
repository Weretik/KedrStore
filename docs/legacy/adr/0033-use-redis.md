# ADR 0033: Use Redis

## Date
2025-06-17

## Status
Accepted

## Context
Redis is an in-memory data structure store widely used for caching, real-time analytics, and message brokering. In the Kedr E-Commerce Platform, Redis will be used to enhance performance by caching frequently accessed data, reducing database load, and enabling efficient communication between services.

## Decision
We decided to use Redis in the project to:

1. Improve application performance by caching frequently accessed data.
2. Reduce database load by storing temporary and frequently queried data in memory.
3. Enable efficient communication between services using Redis Pub/Sub.
4. Align with best practices for scalable and high-performance applications.

## Consequences
### Positive
1. Enhances application performance by reducing database load.
2. Provides a scalable solution for caching and real-time data processing.
3. Simplifies communication between services using Redis Pub/Sub.
4. Widely supported and integrates well with .NET.

### Negative
1. Adds complexity to the infrastructure by introducing an additional service.
2. Requires careful management of cache invalidation to ensure data consistency.
3. May lead to increased operational costs for hosting Redis.

## Example
Redis will be integrated as follows:

**Configuration in appsettings.json**:
```json
{
  "Redis": {
    "ConnectionString": "localhost:6379",
    "DefaultDatabase": 0
  }
}
```

**Registration in DI**:
```csharp
public static class RedisServiceCollectionExtensions
{
    public static IServiceCollection AddRedis(this IServiceCollection services, IConfiguration configuration)
    {
        var redisConfig = configuration.GetSection("Redis").Get<RedisConfig>();
        services.AddSingleton<IConnectionMultiplexer>(ConnectionMultiplexer.Connect(redisConfig.ConnectionString));
        services.AddScoped<ICacheService, RedisCacheService>();
        return services;
    }
}

public class RedisConfig
{
    public string ConnectionString { get; set; }
    public int DefaultDatabase { get; set; }
}
```

**Implementation of ICacheService**:
```csharp
public class RedisCacheService : ICacheService
{
    private readonly IConnectionMultiplexer _connectionMultiplexer;

    public RedisCacheService(IConnectionMultiplexer connectionMultiplexer)
    {
        _connectionMultiplexer = connectionMultiplexer;
    }

    public T Get<T>(string key)
    {
        var db = _connectionMultiplexer.GetDatabase();
        var value = db.StringGet(key);
        return value.IsNullOrEmpty ? default : JsonSerializer.Deserialize<T>(value);
    }

    public async Task<T> GetAsync<T>(string key, CancellationToken cancellationToken = default)
    {
        var db = _connectionMultiplexer.GetDatabase();
        var value = await db.StringGetAsync(key);
        return value.IsNullOrEmpty ? default : JsonSerializer.Deserialize<T>(value);
    }

    public void Set<T>(string key, T value, TimeSpan? expirationTime = null)
    {
        var db = _connectionMultiplexer.GetDatabase();
        var serializedValue = JsonSerializer.Serialize(value);
        db.StringSet(key, serializedValue, expirationTime);
    }

    public async Task SetAsync<T>(string key, T value, TimeSpan? expirationTime = null, CancellationToken cancellationToken = default)
    {
        var db = _connectionMultiplexer.GetDatabase();
        var serializedValue = JsonSerializer.Serialize(value);
        await db.StringSetAsync(key, serializedValue, expirationTime);
    }

    public void Remove(string key)
    {
        var db = _connectionMultiplexer.GetDatabase();
        db.KeyDelete(key);
    }

    public async Task RemoveAsync(string key, CancellationToken cancellationToken = default)
    {
        var db = _connectionMultiplexer.GetDatabase();
        await db.KeyDeleteAsync(key);
    }
}
```
