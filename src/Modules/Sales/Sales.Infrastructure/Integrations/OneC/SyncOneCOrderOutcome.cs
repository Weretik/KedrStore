namespace Sales.Infrastructure.Integrations.OneC;

public enum SyncOneCOrderOutcome
{
    Accepted,
    BusinessError,
    TransportError,
    DeadLetter,
    Failed
}
