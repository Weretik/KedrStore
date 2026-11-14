namespace Sales.Domain.Orders;

public enum OneCOrderSyncStatus
{
    Pending = 0,
    Sent = 1,
    Accepted = 2,
    BusinessError = 3,
    TransportError = 4,
    RetryScheduled = 5,
    DeadLetter = 6
}
