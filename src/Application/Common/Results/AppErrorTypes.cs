namespace Application.Common.Results;

public static class AppErrorTypes
{
    public const string Validation = "ValidationError";
    public const string NotFound = "NotFound";
    public const string Conflict = "Conflict";
    public const string Unauthorized = "Unauthorized";
    public const string Forbidden = "Forbidden";
    public const string Unexpected = "UnexpectedError";
    public const string Integration = "IntegrationError";
    public const string Persistence = "PersistenceError";
    public const string DomainViolation = "DomainViolation";
    public const string Payment = "PaymentError";
    public const string Inventory = "InventoryError";
}
