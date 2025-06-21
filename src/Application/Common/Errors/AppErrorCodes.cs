namespace Application.Common.Errors;

public static class AppErrorCodes
{
    public static class System
    {
        public const string Validation = "System.Validation";
        public const string NotFound = "System.NotFound";
        public const string Conflict = "System.Conflict";
        public const string Unexpected = "System.Unexpected";
        public const string Persistence = "System.Persistence";
        public const string RateLimitExceeded = "System.RateLimitExceeded";
        public const string Unknown = "System.Unknown";
        public const string UnsupportedResponseType = "System.UnsupportedResponseType";
    }

    public static class Seeder
    {
        public const string Failure = "Seeder.Failure";
        public const string DataMissing = "Seeder.DataMissing";
        public const string DbConnection = "Seeder.DbConnection";

    }
    public static class Database
    {
        public const string Unavailable = "Database.Unavailable";
        public const string MigrationFailed = "Database.MigrationFailed";

    }

    public static class File
    {
        public const string WriteError = "File.WriteError";
        public const string ReadError = "File.ReadError";
        public const string NotFound = "File.NotFound";
    }
    public static class Auth
    {
        public const string Unauthorized = "Auth.Unauthorized";
        public const string Forbidden = "Auth.Forbidden";
        public const string UserNotFound = "Auth.UserNotFound";
        public const string EmailAlreadyExists = "Auth.EmailAlreadyExists";
        public const string TokenInvalid = "Auth.TokenInvalid";
        public const string TokenExpired = "Auth.TokenExpired";
    }

    public static class Domain
    {
        public const string Violation = "Domain.DomainViolation";
        public const string Integration = "Domain.Integration";
        public const string Payment = "Domain.Payment";
        public const string Inventory = "Domain.Inventory";
    }

    public static class Catalog
    {
        public const string ProductNotFound = "Catalog.ProductNotFound";
        public const string CategoryNotFound = "Catalog.CategoryNotFound";
        public const string ProductAlreadyExists = "Catalog.ProductAlreadyExists";
        public const string CategoryAlreadyExists = "Catalog.CategoryAlreadyExists";
    }

    public static class Order
    {
        public const string NotFound = "Order.NotFound";
        public const string AlreadyExists = "Order.AlreadyExists";
        public const string ProcessingError = "Order.ProcessingError";
        public const string CancellationError = "Order.CancellationError";
    }

    public static class Storage
    {
        public const string FileUploadFailed = "Storage.FileUploadFailed";
        public const string FileNotFound = "Storage.FileNotFound";
    }

    public static class Service
    {
        public const string ExternalServiceError = "Service.ExternalServiceError";
    }

    public static class Cache
    {
        public const string Miss = "Cache.Miss";
        public const string InvalidationFailed = "Cache.WriteFailed";
    }
}
