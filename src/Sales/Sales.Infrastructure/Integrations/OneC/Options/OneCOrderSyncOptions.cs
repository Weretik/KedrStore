namespace Sales.Infrastructure.Integrations.OneC.Options;

public sealed class OneCOrderSyncOptions
{
    public const string SectionName = "Sales:OneCOrderSync";

    public int BatchSize { get; init; } = 20;

    public TimeSpan InterCallDelay { get; init; } = TimeSpan.FromSeconds(5);

    public int MaximumAttempts { get; init; } = 10;

    public TimeSpan SendingWindowStart { get; init; } = TimeSpan.FromHours(7);

    public TimeSpan SendingWindowEnd { get; init; } = TimeSpan.FromHours(20);

    public TimeSpan StaleSentRecoveryDelay { get; init; } = TimeSpan.FromMinutes(15);
}
