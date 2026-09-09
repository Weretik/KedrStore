using System.Security.Cryptography;
using System.Text;
using System.Text.Json;
using Sales.Application.Integrations.OneC.Contracts;
using Sales.Application.Integrations.OneC.DTOs;
using Sales.Domain.Orders.Entities;
using Sales.Domain.Orders.Enums;
using Sales.Domain.Orders.ValueObjects;
using Sales.Infrastructure.Integrations.OneC.Options;

namespace Sales.Infrastructure.Integrations.OneC.Services;

public sealed class SyncOneCOrdersService(
    SalesDbContext salesDbContext,
    ISalesOneCWriteClient oneCWriteClient,
    DeadLetterNotificationService deadLetterNotificationService,
    IOptions<OneCOrderSyncOptions> options,
    ILogger<SyncOneCOrdersService> logger)
{
    private const int MaximumErrorMessageLength = 1_000;
    private readonly OneCOrderSyncOptions _options = options.Value;

    private static readonly TimeSpan[] TransportRetrySchedule =
    [
        TimeSpan.FromMinutes(5),
        TimeSpan.FromMinutes(15),
        TimeSpan.FromMinutes(30),
        TimeSpan.FromHours(1),
        TimeSpan.FromHours(3)
    ];

    private static readonly TimeZoneInfo KyivsTimeZone = ResolveKyivTimeZone();

    public async Task<SyncOneCOrdersResult> RunAsync(
        CancellationToken cancellationToken = default,
        DateTimeOffset? nowUtc = null)
    {
        var nowUtcDefault = nowUtc ?? DateTimeOffset.UtcNow;
        var removedIdempotencyRecords = await RemoveExpiredIdempotencyRecordsAsync(nowUtcDefault, cancellationToken);

        var dueSyncs = await LoadDueSyncsAsync(nowUtcDefault, _options.BatchSize, cancellationToken);
        if (dueSyncs.Count == 0)
        {
            await deadLetterNotificationService.RunAsync(cancellationToken);
            logger.LogInformation(
                "SyncOneCOrdersJob finished with no due records. RemovedIdempotencyRecords={RemovedIdempotencyRecords}.",
                removedIdempotencyRecords);
            return new SyncOneCOrdersResult(0, 0, 0, 0, 0, 0, removedIdempotencyRecords);
        }

        var orderIds = dueSyncs.Select(sync => sync.OrderId).ToHashSet();
        var orders = await salesDbContext.Orders
            .Include(order => order.Lines)
            .Where(order => orderIds.Contains(order.Id))
            .ToDictionaryAsync(order => order.Id, cancellationToken);

        var result = new SyncOneCOrdersResult(
            Processed: 0,
            Accepted: 0,
            BusinessError: 0,
            TransportError: 0,
            Deferred: 0,
            DeadLetter: 0,
            RemovedIdempotencyRecords: removedIdempotencyRecords);

        var newDeadLetterCount = 0;

        for (var index = 0; index < dueSyncs.Count; index++)
        {
            var sync = dueSyncs[index];
            var correlationId = Guid.NewGuid();
            using var _ = logger.BeginScope(new Dictionary<string, object>
            {
                ["CorrelationId"] = correlationId,
                ["OrderId"] = sync.OrderId.Value,
                ["AttemptCount"] = sync.AttemptCount,
                ["Status"] = sync.Status.ToString()
            });

            if (!orders.TryGetValue(sync.OrderId, out var order))
            {
                logger.LogWarning(
                    "OneC sync skipped because order was not found. OrderId={OrderId}.",
                    sync.OrderId.Value);
                continue;
            }

            if (sync.Status == OneCOrderSyncStatus.Sent && sync.AttemptCount >= _options.MaximumAttempts)
            {
                var movedToDeadLetter = await TryPersistStateAsync(
                    sync,
                    nowUtcDefault,
                    static (target, transitionTime, message) => target.MoveToDeadLetter(transitionTime, message),
                    "StaleSentAttemptTimedOut",
                    cancellationToken);

                if (movedToDeadLetter)
                {
                    result = result with { Processed = result.Processed + 1 };
                    newDeadLetterCount++;
                }

                continue;
            }

            var attemptStartedAt = nowUtcDefault;
            var isInWindow = IsWithinSendingWindow(attemptStartedAt);
            if (!isInWindow)
            {
                var nextAllowedAttemptAtUtc = CalculateNextAllowedWindowStartUtc(attemptStartedAt);
                var deferred = await TryDeferSyncAsync(sync, attemptStartedAt, nextAllowedAttemptAtUtc, cancellationToken);
                if (deferred)
                {
                    result = result with { Processed = result.Processed + 1, Deferred = result.Deferred + 1 };
                }

                logger.LogInformation(
                    "OneC sync deferred out of sending window. OrderId={OrderId}, NextAttemptAtUtc={NextAttemptAtUtc}.",
                    sync.OrderId.Value,
                    sync.NextAttemptAtUtc);
                continue;
            }

            var request = BuildRequest(order);
            var requestHash = CalculateRequestHash(request);
            if (!await TryStartAttemptAsync(sync, requestHash, attemptStartedAt, cancellationToken))
                continue;

            logger.LogInformation(
                "OneC send attempt started. OrderId={OrderId}, OrderNumber={OrderNumber}, Attempt={Attempt}, Status={Status}.",
                sync.OrderId.Value,
                order.OrderNumber,
                sync.AttemptCount,
                sync.Status);

            var writeResult = await oneCWriteClient.SendOrderAsync(request, cancellationToken);
            result = result with { Processed = result.Processed + 1 };

            var outcome = writeResult.Outcome switch
            {
                OneCOrderDeliveryOutcome.Accepted => await ApplyAcceptedAsync(sync, attemptStartedAt, writeResult.OneCDocumentId, cancellationToken),
                OneCOrderDeliveryOutcome.BusinessError => await ApplyBusinessErrorAsync(sync, attemptStartedAt, writeResult.Diagnostic, cancellationToken),
                OneCOrderDeliveryOutcome.TransportError => await ApplyTransportErrorAsync(sync, attemptStartedAt, writeResult.Diagnostic, cancellationToken),
                _ => SyncOneCOrderOutcome.Failed
            };

            if (outcome == SyncOneCOrderOutcome.Accepted)
                result = result with { Accepted = result.Accepted + 1 };
            else if (outcome == SyncOneCOrderOutcome.BusinessError)
                result = result with { BusinessError = result.BusinessError + 1 };
            else if (outcome == SyncOneCOrderOutcome.TransportError)
                result = result with { TransportError = result.TransportError + 1 };
            else if (outcome == SyncOneCOrderOutcome.DeadLetter)
                newDeadLetterCount++;

            if (index + 1 < dueSyncs.Count && dueSyncs[index + 1].NextAttemptAtUtc <= nowUtcDefault)
                await Task.Delay(_options.InterCallDelay, cancellationToken);
        }

        await deadLetterNotificationService.RunAsync(cancellationToken);

        result = result with { DeadLetter = result.DeadLetter + newDeadLetterCount };

        logger.LogInformation(
            "SyncOneCOrdersJob finished. Processed={Processed}; Accepted={Accepted}; BusinessError={BusinessError}; " +
            "TransportError={TransportError}; Deferred={Deferred}; DeadLetter={DeadLetter}; RemovedIdempotencyRecords={RemovedIdempotencyRecords}.",
            result.Processed,
            result.Accepted,
            result.BusinessError,
            result.TransportError,
            result.Deferred,
            result.DeadLetter,
            result.RemovedIdempotencyRecords);

        return result;
    }

    private async Task<IReadOnlyList<OneCOrderSync>> LoadDueSyncsAsync(
        DateTimeOffset nowUtc,
        int batchSize,
        CancellationToken cancellationToken)
    {
        var staleSentBeforeUtc = nowUtc.Subtract(_options.StaleSentRecoveryDelay);

        return await salesDbContext.OneCOrderSyncs
            .Where(sync =>
                (sync.NextAttemptAtUtc <= nowUtc &&
                 (sync.Status == OneCOrderSyncStatus.Pending
                  || sync.Status == OneCOrderSyncStatus.TransportError
                  || sync.Status == OneCOrderSyncStatus.RetryScheduled))
                || (sync.Status == OneCOrderSyncStatus.Sent
                    && sync.LastAttemptAtUtc.HasValue
                    && sync.LastAttemptAtUtc.Value <= staleSentBeforeUtc))
            .OrderBy(sync => sync.NextAttemptAtUtc)
            .ThenBy(sync => sync.Id)
            .Take(batchSize)
            .ToListAsync(cancellationToken);
    }

    private OneCOrderDeliveryRequest BuildRequest(Order order)
        => new(
            order.CounterpartyId,
            order.OrderNumber,
            order.CreatedAt,
            order.Comment,
            [.. order.Lines.Select(line => new OneCOrderDeliveryLineDto(line.ProductId, line.Quantity, line.Amount))]
        );

    private static string CalculateRequestHash(OneCOrderDeliveryRequest request)
    {
        var canonical = JsonSerializer.Serialize(new
        {
            counterpartyId = request.CounterpartyId,
            orderNumber = request.OrderNumber,
            comment = request.Comment,
            lines = request.Lines.Select(line => new
            {
                productId = line.ProductId,
                quantity = line.Quantity,
                amount = line.Amount
            })
        });

        return Convert.ToHexString(SHA256.HashData(Encoding.UTF8.GetBytes(canonical)));
    }

    private async Task<int> RemoveExpiredIdempotencyRecordsAsync(
        DateTimeOffset nowUtc,
        CancellationToken cancellationToken)
    {
        try
        {
            return await salesDbContext.OrderIdempotencyRecords
                .Where(record => record.ExpiresAtUtc <= nowUtc)
                .ExecuteDeleteAsync(cancellationToken);
        }
        catch (Exception exception)
        {
            logger.LogWarning(exception, "Failed to remove expired OrderIdempotencyRecord rows.");
            return 0;
        }
    }

    private async Task<bool> TryDeferSyncAsync(
        OneCOrderSync sync,
        DateTimeOffset nowUtc,
        DateTimeOffset nextAttemptAtUtc,
        CancellationToken cancellationToken)
    {
        try
        {
            sync.Defer(nowUtc, nextAttemptAtUtc);
            await salesDbContext.SaveChangesAsync(cancellationToken);
            return true;
        }
        catch (DbUpdateConcurrencyException exception)
        {
            salesDbContext.Entry(sync).State = EntityState.Detached;
            logger.LogWarning(
                exception,
                "Out-of-window 1C sync was not deferred due concurrent claim. OrderId={OrderId}.",
                sync.OrderId.Value);
            return false;
        }
        catch (Exception exception)
        {
            salesDbContext.Entry(sync).State = EntityState.Detached;
            logger.LogWarning(
                exception,
                "Out-of-window 1C sync could not be deferred. OrderId={OrderId}.",
                sync.OrderId.Value);
            return false;
        }
    }

    private async Task<bool> TryStartAttemptAsync(
        OneCOrderSync sync,
        string requestPayloadHash,
        DateTimeOffset nowUtc,
        CancellationToken cancellationToken)
    {
        try
        {
            sync.StartAttempt(nowUtc, requestPayloadHash);
            await salesDbContext.SaveChangesAsync(cancellationToken);
            return true;
        }
        catch (DbUpdateConcurrencyException exception)
        {
            salesDbContext.Entry(sync).State = EntityState.Detached;
            logger.LogWarning(
                exception,
                "OneC send attempt claim lost due concurrency. OrderId={OrderId}.",
                sync.OrderId.Value);
            return false;
        }
        catch (Exception exception)
        {
            salesDbContext.Entry(sync).State = EntityState.Detached;
            logger.LogWarning(
                exception,
                "Failed to claim OneC send attempt. OrderId={OrderId}.",
                sync.OrderId.Value);
            return false;
        }
    }

    private async Task<SyncOneCOrderOutcome> ApplyAcceptedAsync(
        OneCOrderSync sync,
        DateTimeOffset nowUtc,
        string? documentId,
        CancellationToken cancellationToken)
    {
        if (string.IsNullOrWhiteSpace(documentId))
            return await ApplyBusinessErrorAsync(sync, nowUtc, "OneCDocumentWasNotCreated", cancellationToken);

        var persisted = await TryPersistStateAsync(
            sync,
            nowUtc,
            static (target, attemptTime, document) => target.Accept(attemptTime, document),
            documentId,
            cancellationToken);

        if (!persisted)
            return SyncOneCOrderOutcome.Failed;

        logger.LogInformation(
            "OneC order accepted. OrderId={OrderId}, Attempt={Attempt}, DocumentId={DocumentId}.",
            sync.OrderId.Value,
            sync.AttemptCount,
            documentId);

        return SyncOneCOrderOutcome.Accepted;
    }

    private async Task<SyncOneCOrderOutcome> ApplyBusinessErrorAsync(
        OneCOrderSync sync,
        DateTimeOffset nowUtc,
        string? diagnostic,
        CancellationToken cancellationToken)
    {
        var sanitized = SanitizeMessage(diagnostic);
        var persisted = await TryPersistStateAsync(
            sync,
            nowUtc,
            static (target, attemptTime, message) => target.MarkBusinessError(attemptTime, message),
            sanitized,
            cancellationToken);

        if (!persisted)
            return SyncOneCOrderOutcome.Failed;

        logger.LogWarning(
            "OneC business rejection. OrderId={OrderId}, Attempt={Attempt}, Error={Error}.",
            sync.OrderId.Value,
            sync.AttemptCount,
            sanitized);

        return SyncOneCOrderOutcome.BusinessError;
    }

    private async Task<SyncOneCOrderOutcome> ApplyTransportErrorAsync(
        OneCOrderSync sync,
        DateTimeOffset nowUtc,
        string? diagnostic,
        CancellationToken cancellationToken)
    {
        var sanitized = SanitizeMessage(diagnostic);
        var nextAttemptAtUtc = CalculateNextTransportRetry(sync.AttemptCount, nowUtc);

        var persisted = await TryPersistStateAsync(
            sync,
            nowUtc,
            (target, attemptTime, payload) => target.RegisterTransportFailure(
                attemptTime,
                payload.NextAttemptAtUtc,
                _options.MaximumAttempts,
                "TransportError",
                payload.Diagnostic,
                httpStatus: null),
            (NextAttemptAtUtc: nextAttemptAtUtc, Diagnostic: sanitized),
            cancellationToken);

        if (!persisted)
            return SyncOneCOrderOutcome.Failed;

        logger.LogWarning(
            "OneC transport failure. OrderId={OrderId}, Attempt={Attempt}, Status={Status}, NextAttemptAtUtc={NextAttemptAtUtc}, Error={Error}.",
            sync.OrderId.Value,
            sync.AttemptCount,
            sync.Status,
            sync.NextAttemptAtUtc,
            sanitized);

        return sync.Status == OneCOrderSyncStatus.DeadLetter
            ? SyncOneCOrderOutcome.DeadLetter
            : SyncOneCOrderOutcome.TransportError;
    }

    private async Task<bool> TryPersistStateAsync<TState>(
        OneCOrderSync sync,
        DateTimeOffset nowUtc,
        Action<OneCOrderSync, DateTimeOffset, TState> transition,
        TState state,
        CancellationToken cancellationToken)
    {
        try
        {
            transition(sync, nowUtc, state);
            await salesDbContext.SaveChangesAsync(cancellationToken);
            return true;
        }
        catch (DbUpdateConcurrencyException exception)
        {
            salesDbContext.Entry(sync).State = EntityState.Detached;
            logger.LogWarning(
                exception,
                "OneC sync state update was lost due concurrency. OrderId={OrderId}.",
                sync.OrderId.Value);
            return false;
        }
        catch (Exception exception)
        {
            salesDbContext.Entry(sync).State = EntityState.Detached;
            logger.LogWarning(
                exception,
                "Failed to persist OneC sync state. OrderId={OrderId}.",
                sync.OrderId.Value);
            return false;
        }
    }

    private DateTimeOffset CalculateNextAllowedWindowStartUtc(DateTimeOffset nowUtc)
    {
        var localNow = TimeZoneInfo.ConvertTime(nowUtc, KyivsTimeZone);
        var localDate = localNow.Date;

        if (localNow.TimeOfDay < _options.SendingWindowStart)
            return new DateTimeOffset(
                TimeZoneInfo.ConvertTimeToUtc(localDate + _options.SendingWindowStart, KyivsTimeZone),
                TimeSpan.Zero);

        if (localNow.TimeOfDay >= _options.SendingWindowEnd)
            return new DateTimeOffset(
                TimeZoneInfo.ConvertTimeToUtc(localDate.AddDays(1) + _options.SendingWindowStart, KyivsTimeZone),
                TimeSpan.Zero);

        return nowUtc;
    }

    private bool IsWithinSendingWindow(DateTimeOffset nowUtc)
    {
        var localNow = TimeZoneInfo.ConvertTime(nowUtc, KyivsTimeZone);
        return localNow.TimeOfDay >= _options.SendingWindowStart && localNow.TimeOfDay < _options.SendingWindowEnd;
    }

    private static DateTimeOffset CalculateNextTransportRetry(int attemptCount, DateTimeOffset nowUtc)
    {
        if (attemptCount <= 0)
            return nowUtc;

        var index = attemptCount - 1;
        if (index >= TransportRetrySchedule.Length)
            return nowUtc.Add(TransportRetrySchedule[^1]);

        return nowUtc.Add(TransportRetrySchedule[index]);
    }

    private static string SanitizeMessage(string? message)
    {
        if (string.IsNullOrWhiteSpace(message))
            return "Transport error";

        var normalized = new string(message
            .Where(character => !char.IsControl(character))
            .ToArray());

        return normalized.Length <= MaximumErrorMessageLength
            ? normalized
            : normalized[..MaximumErrorMessageLength];
    }

    private static TimeZoneInfo ResolveKyivTimeZone()
    {
        try
        {
            return TimeZoneInfo.FindSystemTimeZoneById("Europe/Kyiv");
        }
        catch (TimeZoneNotFoundException)
        {
            try
            {
                return TimeZoneInfo.FindSystemTimeZoneById("Europe/Kiev");
            }
            catch (TimeZoneNotFoundException)
            {
                return TimeZoneInfo.FindSystemTimeZoneById("FLE Standard Time");
            }
        }
        catch (InvalidTimeZoneException)
        {
            return TimeZoneInfo.Utc;
        }
    }
}
