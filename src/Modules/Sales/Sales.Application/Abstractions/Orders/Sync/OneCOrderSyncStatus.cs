namespace Sales.Application.Abstractions.Orders.Sync;

/// <summary>
/// The 1C state of an order as the admin and the mobile app see it (platform spec 006, R-012). The names are the API
/// values; <c>Sent</c> and <c>TransportError</c> of the old sync are no longer produced.
/// </summary>
public enum OneCOrderSyncStatus
{
    Pending = 0,
    Accepted = 2,
    BusinessError = 3,
    RetryScheduled = 5,
    DeadLetter = 6
}
