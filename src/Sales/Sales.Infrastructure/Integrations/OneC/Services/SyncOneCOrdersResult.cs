namespace Sales.Infrastructure.Integrations.OneC.Services;

public sealed record SyncOneCOrdersResult(
    int Processed,
    int Accepted,
    int BusinessError,
    int TransportError,
    int Deferred,
    int DeadLetter,
    int RemovedIdempotencyRecords);
