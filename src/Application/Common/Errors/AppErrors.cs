namespace Application.Common.Errors;

public static class AppErrors
{
    public static class System
    {
        public static AppError Validation(string message) =>
            new(AppErrorCodes.System.Validation,
                $"{AppErrorDescriptions.System.Validation}  {message}");

        public static AppError NotFound =>
            new(AppErrorCodes.System.NotFound,
                AppErrorDescriptions.System.NotFound);

        public static AppError Conflict =>
            new(AppErrorCodes.System.Conflict,
                AppErrorDescriptions.System.Conflict);

        public static AppError Unexpected =>
            new(AppErrorCodes.System.Unexpected,
                AppErrorDescriptions.System.Unexpected);

        public static AppError Persistence =>
            new(AppErrorCodes.System.Persistence,
                AppErrorDescriptions.System.Persistence);

        public static AppError RateLimitExceeded =>
            new(AppErrorCodes.System.RateLimitExceeded,
                AppErrorDescriptions.System.RateLimitExceeded);

        public static AppError Unknown =>
            new AppError(AppErrorCodes.System.Unknown,
                    AppErrorDescriptions.System.Unknown);

        public static AppError UnsupportedResponseType =>
            new(AppErrorCodes.System.UnsupportedResponseType,
                AppErrorDescriptions.System.UnsupportedResponseType);
    }
    public static class Seeder
    {
        public static readonly AppError Failure =
            new(AppErrorCodes.Seeder.Failure,
                AppErrorDescriptions.Seeder.Failure);

        public static readonly AppError DataMissing =
            new(AppErrorCodes.Seeder.DataMissing,
                AppErrorDescriptions.Seeder.DataMissing);

        public static readonly AppError DbConnection =
            new(AppErrorCodes.Seeder.DbConnection,
                AppErrorDescriptions.Seeder.DbConnection);
    }
    public static class Database
    {
        public static AppError Unavailable =>
            new(AppErrorCodes.Database.Unavailable,
                AppErrorDescriptions.Database.Unavailable);

        public static AppError MigrationFailed =>
            new(AppErrorCodes.Database.MigrationFailed,
                AppErrorDescriptions.Database.MigrationFailed);
    }
    public static class File
    {
        public static AppError WriteError =>
            new(AppErrorCodes.File.WriteError,
                AppErrorDescriptions.File.WriteError);

        public static AppError ReadError =>
            new(AppErrorCodes.File.ReadError,
                AppErrorDescriptions.File.ReadError);

        public static AppError NotFound =>
            new(AppErrorCodes.File.NotFound,
                AppErrorDescriptions.File.NotFound);
    }
    public static class Auth
    {
        public static AppError Unauthorized =>
            new (AppErrorCodes.Auth.Unauthorized,
                    AppErrorDescriptions.Auth.Unauthorized);
        public static AppError Forbidden =>
            new AppError(AppErrorCodes.Auth.Forbidden,
                    AppErrorDescriptions.Auth.Forbidden);
        public static AppError UserNotFound =>
            new(AppErrorCodes.Auth.UserNotFound,
                AppErrorDescriptions.Auth.UserNotFound);
        public static AppError EmailAlreadyExists =>
            new(AppErrorCodes.Auth.EmailAlreadyExists,
                AppErrorDescriptions.Auth.EmailAlreadyExists);
        public static AppError TokenInvalid =>
            new(AppErrorCodes.Auth.TokenInvalid,
                AppErrorDescriptions.Auth.TokenInvalid);
        public static AppError TokenExpired =>
            new(AppErrorCodes.Auth.TokenExpired,
                AppErrorDescriptions.Auth.TokenExpired);
    }
    public static class Domain
    {
        public static AppError Violation =>
            new(AppErrorCodes.Domain.Violation,
                AppErrorDescriptions.Domain.Violation);

        public static AppError Integration =>
            new(AppErrorCodes.Domain.Integration,
                AppErrorDescriptions.Domain.Integration);


        public static AppError Payment =>
            new(AppErrorCodes.Domain.Payment,
                    AppErrorDescriptions.Domain.Payment);

        public static AppError Inventory =>
            new(AppErrorCodes.Domain.Inventory,
                    AppErrorDescriptions.Domain.Inventory);

    }
    public static class Catalog
    {
        public static AppError ProductNotFound =>
            new(AppErrorCodes.Catalog.ProductNotFound,
                AppErrorDescriptions.Catalog.ProductNotFound);

        public static AppError CategoryNotFound =>
            new(AppErrorCodes.Catalog.CategoryNotFound,
                AppErrorDescriptions.Catalog.CategoryNotFound);

        public static AppError ProductAlreadyExists =>
            new(AppErrorCodes.Catalog.ProductAlreadyExists,
                AppErrorDescriptions.Catalog.ProductAlreadyExists);

        public static AppError CategoryAlreadyExists() =>
            new(AppErrorCodes.Catalog.CategoryAlreadyExists,
                AppErrorDescriptions.Catalog.CategoryAlreadyExists);
    }
    public static class Order
    {
        public static AppError NotFound =>
            new(AppErrorCodes.Order.NotFound,
                AppErrorDescriptions.Order.NotFound);

        public static AppError AlreadyExists =>
            new(AppErrorCodes.Order.AlreadyExists,
                AppErrorDescriptions.Order.AlreadyExists);

        public static AppError ProcessingError =>
            new AppError(AppErrorCodes.Order.ProcessingError,
                AppErrorDescriptions.Order.ProcessingError);

        public static AppError CancellationError =>
            new AppError(AppErrorCodes.Order.CancellationError,
                AppErrorDescriptions.Order.CancellationError);
    }
    public static class Storage
    {
        public static AppError FileUploadFailed =>
            new(AppErrorCodes.Storage.FileUploadFailed,
                AppErrorDescriptions.Storage.FileUploadFailed);

        public static AppError FileNotFound =>
            new AppError(AppErrorCodes.Storage.FileNotFound,
                    AppErrorDescriptions.Storage.FileNotFound);
    }
    public static class Service
    {
        public static AppError ExternalServiceError =>
            new AppError(AppErrorCodes.Service.ExternalServiceError,
                AppErrorDescriptions.Service.ExternalServiceError);
    }
    public static class Cache
    {
        public static AppError CacheMiss =>
            new(AppErrorCodes.Cache.Miss,
                AppErrorDescriptions.Cache.Miss);

        public static AppError CacheInvalidationFailed =>
            new(AppErrorCodes.Cache.InvalidationFailed,
                AppErrorDescriptions.Cache.InvalidationFailed);
    }

}

