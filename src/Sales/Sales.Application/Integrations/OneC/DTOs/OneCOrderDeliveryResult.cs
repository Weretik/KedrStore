namespace Sales.Application.Integrations.OneC.DTOs;

public enum OneCOrderDeliveryOutcome
{
    Accepted,
    BusinessError,
    TransportError
}

public sealed record OneCOrderDeliveryResult(
    OneCOrderDeliveryOutcome Outcome,
    string? OneCDocumentId,
    string? Diagnostic)
{
    public static OneCOrderDeliveryResult Accepted(string documentId, string? diagnostic)
        => new(OneCOrderDeliveryOutcome.Accepted, documentId, diagnostic);

    public static OneCOrderDeliveryResult BusinessError(string? diagnostic)
        => new(OneCOrderDeliveryOutcome.BusinessError, null, diagnostic);

    public static OneCOrderDeliveryResult TransportError(string diagnostic)
        => new(OneCOrderDeliveryOutcome.TransportError, null, diagnostic);
}
