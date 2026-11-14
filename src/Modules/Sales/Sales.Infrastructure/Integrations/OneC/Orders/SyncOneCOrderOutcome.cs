namespace Sales.Infrastructure.Integrations.OneC.Orders;

public enum SyncOneCOrderOutcome
{
    Accepted,
    BusinessError,
    TransportError,
    DeadLetter,
    Failed
}
