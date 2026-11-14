using BuildingBlocks.Application.Outbox;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Npgsql;
using BuildingBlocks.Infrastructure.Outbox.Persistence;
namespace BuildingBlocks.Infrastructure.Outbox.Dispatch;

/// <summary>
/// Sends the due messages of one module's table: locks a small batch with <c>FOR UPDATE SKIP LOCKED</c>, so two
/// dispatches never send the same message at once, runs each handler and records the outcome (platform spec 006, R-004).
/// </summary>
internal sealed class ModuleOutbox<TContext>(
    IServiceScopeFactory scopes,
    OutboxMessageTypes types,
    TimeProvider clock,
    ILogger<ModuleOutbox<TContext>> logger,
    string schema) : IModuleOutbox
    where TContext : DbContext
{
    internal const int BatchSize = 50;

    public async Task<OutboxDispatchReport> ProcessAsync(
        IReadOnlyCollection<Guid>? messageIds,
        CancellationToken cancellationToken)
    {
        await using var scope = scopes.CreateAsyncScope();
        var dbContext = scope.ServiceProvider.GetRequiredService<TContext>();
        var nowUtc = clock.GetUtcNow();

        await using var transaction = await dbContext.Database.BeginTransactionAsync(cancellationToken);
        var messages = await LockDue(dbContext, nowUtc, messageIds).ToListAsync(cancellationToken);
        if (messages.Count == 0)
            return OutboxDispatchReport.Empty;

        var report = OutboxDispatchReport.Empty;
        foreach (var message in messages)
            report = report.Add(await SendAsync(scope.ServiceProvider, message, nowUtc, cancellationToken));

        await dbContext.SaveChangesAsync(cancellationToken);
        await transaction.CommitAsync(cancellationToken);
        return report;
    }

    public async Task<int> RemoveProcessedAsync(CancellationToken cancellationToken)
    {
        await using var scope = scopes.CreateAsyncScope();
        var dbContext = scope.ServiceProvider.GetRequiredService<TContext>();
        var cutoff = clock.GetUtcNow() - OutboxRetrySchedule.Retention;
        return await dbContext.Set<OutboxMessage>()
            .Where(message => message.ProcessedAtUtc != null && message.ProcessedAtUtc < cutoff)
            .ExecuteDeleteAsync(cancellationToken);
    }

    private async Task<OutboxDispatchReport> SendAsync(
        IServiceProvider services,
        OutboxMessage message,
        DateTimeOffset nowUtc,
        CancellationToken cancellationToken)
    {
        try
        {
            var type = types.Find(message.Type)
                ?? throw new InvalidOperationException($"Outbox message type '{message.Type}' has no handler.");
            await type.HandleAsync(services, message.Payload, message.Id, cancellationToken);
            message.MarkProcessed(nowUtc);
            return new OutboxDispatchReport(1, 0, 0);
        }
        catch (Exception exception) when (!cancellationToken.IsCancellationRequested)
        {
            if (message.MarkAttemptFailed(nowUtc, exception.Message))
            {
                logger.LogError(exception,
                    "Outbox message {MessageId} of type {MessageType} in {Schema} failed after {Attempts} attempts and will not be retried.",
                    message.Id, message.Type, schema, message.Attempts);
                return new OutboxDispatchReport(0, 0, 1);
            }

            logger.LogWarning(exception,
                "Outbox message {MessageId} of type {MessageType} in {Schema} failed attempt {Attempts}; next attempt at {NextAttemptAtUtc}.",
                message.Id, message.Type, schema, message.Attempts, message.NextAttemptAtUtc);
            return new OutboxDispatchReport(0, 1, 0);
        }
    }

    private IQueryable<OutboxMessage> LockDue(
        TContext dbContext,
        DateTimeOffset nowUtc,
        IReadOnlyCollection<Guid>? messageIds)
    {
        var onlyGiven = messageIds is null ? string.Empty : """ AND "Id" = ANY(@ids)""";
        var sql = $"""
            SELECT * FROM "{schema}"."{OutboxModelBuilderExtensions.TableName}"
            WHERE "ProcessedAtUtc" IS NULL AND "FailedAtUtc" IS NULL AND "NextAttemptAtUtc" <= @now{onlyGiven}
            ORDER BY "NextAttemptAtUtc"
            LIMIT {BatchSize}
            FOR UPDATE SKIP LOCKED
            """;

        var parameters = new List<object> { new NpgsqlParameter("now", nowUtc) };
        if (messageIds is not null)
            parameters.Add(new NpgsqlParameter("ids", messageIds.ToArray()));

        return dbContext.Set<OutboxMessage>().FromSqlRaw(sql, parameters.ToArray());
    }
}
