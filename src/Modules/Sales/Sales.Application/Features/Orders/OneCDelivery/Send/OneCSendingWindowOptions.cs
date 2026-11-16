namespace Sales.Application.Features.Orders.OneCDelivery.Send;

/// <summary>When 1C takes orders, in Kyiv time (platform spec 006, R-011).</summary>
public sealed class OneCSendingWindowOptions
{
    public const string SectionName = "Sales:OneCOrderSync";

    public TimeSpan SendingWindowStart { get; init; } = TimeSpan.FromHours(7);

    /// <summary>Exclusive.</summary>
    public TimeSpan SendingWindowEnd { get; init; } = TimeSpan.FromHours(20);
}
