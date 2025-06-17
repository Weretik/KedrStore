namespace Application.Common.Results;

public static class AppErrorTypes
{
    public static class System
    {
        public const string Validation = "Validation";
        public const string NotFound = "NotFound";
        public const string Conflict = "Conflict";
        public const string Unexpected = "Unexpected";
        public const string Persistence = "Persistence";
    }

    public static class Auth
    {
        public const string Unauthorized = "Unauthorized";
        public const string Forbidden = "Forbidden";
    }

    public static class Domain
    {
        public const string DomainViolation = "DomainViolation";
        public const string Integration = "Integration";
        public const string Payment = "Payment";
        public const string Inventory = "Inventory";
    }
}
