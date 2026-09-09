namespace Sales.Infrastructure.Integrations.OneC.Services;

public sealed class DeadLetterNotificationService(
    SalesDbContext salesDbContext,
    IOrderDeliveryFailureExporter exporter,
    IDeadLetterNotifier notifier,
    ILogger<DeadLetterNotificationService> logger)
{
    private static readonly TimeSpan[] NotificationRetrySchedule =
    [
        TimeSpan.FromMinutes(5),
        TimeSpan.FromMinutes(15),
        TimeSpan.FromMinutes(30),
        TimeSpan.FromHours(1),
        TimeSpan.FromHours(3)
    ];

    private const int DefaultMaximumAttempts = 5;
    private const int DefaultBatchSize = 20;

    public async Task<int> RunAsync(
        CancellationToken cancellationToken = default,
        int maximumAttempts = DefaultMaximumAttempts,
        int batchSize = DefaultBatchSize)
    {
        var now = DateTimeOffset.UtcNow;
        var syncs = await salesDbContext.OneCOrderSyncs
            .Where(sync =>
                sync.Status == Sales.Domain.Orders.Enums.OneCOrderSyncStatus.DeadLetter &&
                sync.DeadLetterNotifiedAtUtc == null &&
                sync.DeadLetterNotificationExhaustedAtUtc == null &&
                sync.DeadLetterNotificationAttemptCount < maximumAttempts &&
                (sync.DeadLetterNotificationNextAttemptAtUtc == null || sync.DeadLetterNotificationNextAttemptAtUtc <= now))
            .OrderBy(sync => sync.DeadLetterNotificationNextAttemptAtUtc)
            .Take(batchSize)
            .ToListAsync(cancellationToken);

        var processed = 0;

        foreach (var sync in syncs)
        {
            var claimed = await TryClaimNotificationAttemptAsync(sync, now, maximumAttempts, cancellationToken);
            if (!claimed)
                continue;

            processed++;

            var notification = await BuildNotificationAsync(sync, maximumAttempts, cancellationToken);
            if (notification is null)
            {
                continue;
            }

            try
            {
                var attachment = exporter.Build(notification);
                await notifier.SendAsync(notification, attachment, cancellationToken);

                sync.MarkDeadLetterNotified(DateTimeOffset.UtcNow);
                await salesDbContext.SaveChangesAsync(cancellationToken);

                logger.LogInformation(
                    "Dead-letter notification sent for OrderId={OrderId}, OrderNumber={OrderNumber}, Attempt={Attempt}.",
                    notification.OrderId,
                    notification.OrderNumber,
                    sync.DeadLetterNotificationAttemptCount);
            }
            catch (Exception ex)
            {
                var failureMessage = SanitizeFailureMessage(ex);
                var nextAttempt = CalculateNextAttempt(DateTimeOffset.UtcNow, sync.DeadLetterNotificationAttemptCount, maximumAttempts);

                sync.RegisterDeadLetterNotificationFailure(
                    DateTimeOffset.UtcNow,
                    nextAttempt,
                    maximumAttempts,
                    failureMessage);

                try
                {
                    await salesDbContext.SaveChangesAsync(cancellationToken);
                }
                catch (Exception saveException)
                {
                    logger.LogWarning(
                        saveException,
                        "Failed to persist dead-letter notification failure metadata for OrderId={OrderId} after send failure.",
                        notification.OrderId);
                }

                logger.LogWarning(
                    ex,
                    "Dead-letter notification failed for OrderId={OrderId}, Attempt={Attempt}, next_attempt_at={NextAttempt}.",
                    notification.OrderId,
                    sync.DeadLetterNotificationAttemptCount,
                    nextAttempt);
            }
        }

        return processed;
    }

    private static DateTimeOffset? CalculateNextAttempt(
        DateTimeOffset now,
        int notificationAttemptCount,
        int maximumAttempts)
    {
        if (notificationAttemptCount >= maximumAttempts)
            return null;

        var index = notificationAttemptCount - 1;
        return index >= 0 && index < NotificationRetrySchedule.Length
            ? now.Add(NotificationRetrySchedule[index])
            : null;
    }

    private async Task<bool> TryClaimNotificationAttemptAsync(
        OneCOrderSync sync,
        DateTimeOffset now,
        int maximumAttempts,
        CancellationToken cancellationToken)
    {
        if (!sync.TryStartDeadLetterNotification(now, maximumAttempts))
            return false;

        try
        {
            await salesDbContext.SaveChangesAsync(cancellationToken);
            return true;
        }
        catch (DbUpdateConcurrencyException ex)
        {
            salesDbContext.Entry(sync).State = EntityState.Detached;
            logger.LogWarning(ex, "Dead-letter notification claim lost for OrderId={OrderId}; another worker handled it.", sync.OrderId.Value);
            return false;
        }
        catch (Exception ex)
        {
            salesDbContext.Entry(sync).State = EntityState.Detached;
            logger.LogWarning(ex, "Failed to claim dead-letter notification attempt for OrderId={OrderId}.", sync.OrderId.Value);
            return false;
        }
    }

    private async Task<DeadLetterNotification?> BuildNotificationAsync(
        OneCOrderSync sync,
        int maximumAttempts,
        CancellationToken cancellationToken)
    {
        var order = await salesDbContext.Orders
            .Include(order => order.Lines)
            .FirstOrDefaultAsync(order => order.Id == sync.OrderId, cancellationToken);

        if (order is null)
        {
            var message = $"Order not found for dead-letter sync OrderId={sync.OrderId.Value}.";
            await HandleMissingOrderDataAsync(sync, message, maximumAttempts, cancellationToken);
            return null;
        }

        var counterparty = await salesDbContext.Counterparties
            .FirstOrDefaultAsync(item => item.Id == order.CounterpartyId, cancellationToken);
        if (counterparty is null)
        {
            var message = $"Counterparty not found for dead-letter sync OrderId={sync.OrderId.Value}, CounterpartyId={order.CounterpartyId}.";
            await HandleMissingOrderDataAsync(sync, message, maximumAttempts, cancellationToken);
            return null;
        }

        return new DeadLetterNotification(
            sync.OrderId.Value,
            order.OrderNumber,
            counterparty.Id,
            counterparty.Name,
            sync.DeadLetterNotificationLastError ?? sync.LastErrorMessage,
            sync.AttemptCount,
            sync.DeadLetterNotificationLastAttemptAtUtc ?? DateTimeOffset.UtcNow,
            [..order.Lines
                .Select(line => new DeadLetterOrderLine(line.ProductId, line.ProductName, line.Quantity, line.Amount))]);

    }

    private async Task HandleMissingOrderDataAsync(
        OneCOrderSync sync,
        string message,
        int maximumAttempts,
        CancellationToken cancellationToken)
    {
        var failureMessage = message.Length > 1_000 ? message[..1_000] : message;
        var nextAttempt = CalculateNextAttempt(
            DateTimeOffset.UtcNow,
            sync.DeadLetterNotificationAttemptCount,
            maximumAttempts);

        sync.RegisterDeadLetterNotificationFailure(
            DateTimeOffset.UtcNow,
            nextAttempt,
            maximumAttempts,
            failureMessage);

        try
        {
            await salesDbContext.SaveChangesAsync(cancellationToken);
        }
        catch (Exception ex)
        {
            logger.LogWarning(ex, "Failed to persist dead-letter retry state after missing-order data for OrderId={OrderId}.", sync.OrderId.Value);
        }

        logger.LogWarning("Dead-letter retry state update skipped because of invalid dead-letter data: {Message}", message);
    }

    private static string SanitizeFailureMessage(Exception exception)
        => string.IsNullOrWhiteSpace(exception.Message)
            ? "Dead-letter notification failed."
            : exception.Message.Trim();
}
