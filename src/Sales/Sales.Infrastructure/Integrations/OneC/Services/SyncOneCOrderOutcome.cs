namespace Sales.Infrastructure.Integrations.OneC.Services;

public enum SyncOneCOrderOutcome
{
    Accepted,
    BusinessError,
    TransportError,
    DeadLetter,
    Failed
}
