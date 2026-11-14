using System.Text.RegularExpressions;
using BuildingBlocks.Application.Outbox;
using BuildingBlocks.Infrastructure.Outbox;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Microsoft.Extensions.Logging;

namespace BuildingBlocks.Infrastructure.DependencyInjection;

/// <summary>Registers a module's outbox table and its message handlers (platform spec 006).</summary>
public static partial class OutboxRegistration
{
    /// <param name="schema">The module's schema; its model maps the table with <c>ApplyOutbox(schema)</c>.</param>
    public static IServiceCollection AddOutbox<TContext>(this IServiceCollection services, string schema)
        where TContext : DbContext
    {
        if (!SchemaName().IsMatch(schema))
            throw new ArgumentException($"Outbox schema '{schema}' must be lower case letters and underscores.", nameof(schema));

        services.AddOutboxCore();
        services.AddSingleton<IModuleOutbox>(provider => new ModuleOutbox<TContext>(
            provider.GetRequiredService<IServiceScopeFactory>(),
            provider.GetRequiredService<OutboxMessageTypes>(),
            provider.GetRequiredService<TimeProvider>(),
            provider.GetRequiredService<ILogger<ModuleOutbox<TContext>>>(),
            schema));
        return services;
    }

    /// <param name="name">The stored type name; keep it when the message class is renamed.</param>
    public static IServiceCollection AddOutboxHandler<TMessage, THandler>(this IServiceCollection services, string name)
        where TMessage : IOutboxMessage
        where THandler : class, IOutboxMessageHandler<TMessage>
    {
        services.AddOutboxCore();
        services.AddSingleton(OutboxMessageType.For<TMessage>(name));
        services.AddScoped<IOutboxMessageHandler<TMessage>, THandler>();
        return services;
    }

    private static void AddOutboxCore(this IServiceCollection services)
    {
        services.TryAddSingleton(TimeProvider.System);
        services.TryAddSingleton<OutboxMessageTypes>();
        services.TryAddSingleton<OutboxWriter>();
        services.TryAddSingleton<IOutboxDispatcher, OutboxDispatcher>();
    }

    [GeneratedRegex("^[a-z_]+$")]
    private static partial Regex SchemaName();
}
