namespace BuildingBlocks.Application.Outbox;

public sealed record OutboxDispatchReport(int Sent, int Retried, int Failed)
{
    public static OutboxDispatchReport Empty { get; } = new(0, 0, 0);

    public OutboxDispatchReport Add(OutboxDispatchReport other)
        => new(Sent + other.Sent, Retried + other.Retried, Failed + other.Failed);
}
