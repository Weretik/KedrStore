namespace Application.Common.Errors;

public static class AppErrors
{
    public static class System
    {
        public static AppError Validation(string message, string? details = null)
        {
            var error = new AppError(AppErrorCodes.System.Validation, message);
            return details is null ? error : error.WithDetails(details);
        }

        public static AppError NotFound(string entityName, string? id = null)
        {
            var message = string.Format(AppErrorDescriptions.System.NotFound + $"{entityName} {id}");
            return new(AppErrorCodes.System.NotFound, message);
        }

        public static AppError Conflict(string message) =>
            new(AppErrorCodes.System.Conflict, message);

        public static AppError Unexpected(string? details = null) =>
            new AppError(AppErrorCodes.System.Unexpected, AppErrorDescriptions.System.Unexpected)
                .WithDetails(details ?? "Невідома помилка.");

        public static AppError Persistence(string operation, string details)
        {
            var message = string.Format(AppErrorDescriptions.System.Persistence + operation);
            return new AppError(AppErrorCodes.System.Persistence, message)
                .WithDetails(details);
        }
        public static AppError RateLimitExceeded(string? details = null) =>
            new AppError(AppErrorCodes.System.RateLimitExceeded,
                    AppErrorDescriptions.System.RateLimitExceeded)
                .WithDetails(details ?? "Перевищено ліміт запитів.");
        public static AppError Unknown(string? details = null) =>
            new AppError(AppErrorCodes.System.Unknown,
                    AppErrorDescriptions.System.Unknown)
                .WithDetails(details ?? "Невідома системна помилка.");
    }

    public static class Auth
    {
        public static AppError Unauthorized(string? reason = null) =>
            new AppError(AppErrorCodes.Auth.Unauthorized, AppErrorDescriptions.Auth.Unauthorized)
                .WithDetails(reason ?? "Немає доступу.");
        public static AppError Forbidden(string? reason = null) =>
            new AppError(AppErrorCodes.Auth.Forbidden, AppErrorDescriptions.Auth.Forbidden)
                .WithDetails(reason ?? "Заборонено.");
        public static AppError UserNotFound(string email) =>
            new(AppErrorCodes.Auth.UserNotFound,
                string.Format(AppErrorDescriptions.Auth.UserNotFound + email));
        public static AppError EmailAlreadyExists(string email) =>
            new(AppErrorCodes.Auth.EmailAlreadyExists,
                string.Format(AppErrorDescriptions.Auth.EmailAlreadyExists + email));
        public static AppError TokenInvalid(string token) =>
            new(AppErrorCodes.Auth.TokenInvalid,
                string.Format(AppErrorDescriptions.Auth.TokenInvalid + token));
        public static AppError TokenExpired(string token) =>
            new(AppErrorCodes.Auth.TokenExpired,
                string.Format(AppErrorDescriptions.Auth.TokenExpired + token));
    }

    public static class Domain
    {
        public static AppError Violation(string message) =>
            new(AppErrorCodes.Domain.Violation, message);

        public static AppError Integration(string serviceName, string details) =>
            new AppError(AppErrorCodes.Domain.Integration,
                string.Format(AppErrorDescriptions.Domain.Integration + serviceName))
                .WithDetails(details);

        public static AppError Payment(string details) =>
            new AppError(AppErrorCodes.Domain.Payment,
                    AppErrorDescriptions.Domain.Payment)
                .WithDetails(details);

        public static AppError Inventory(string details) =>
            new AppError(AppErrorCodes.Domain.Inventory,
                    AppErrorDescriptions.Domain.Inventory)
                .WithDetails(details);

    }
    public static class Catalog
    {
        public static AppError ProductNotFound(int productId) =>
            new(AppErrorCodes.Catalog.ProductNotFound,
                $"По ID {productId} " + AppErrorDescriptions.Catalog.ProductNotFound);

        public static AppError CategoryNotFound(int categoryId) =>
            new(AppErrorCodes.Catalog.CategoryNotFound,
                $"По ID {categoryId} " + AppErrorDescriptions.Catalog.CategoryNotFound);

        public static AppError ProductAlreadyExists(string productName) =>
            new(AppErrorCodes.Catalog.ProductAlreadyExists,
                $"Товар з назвою '{productName}'. " + AppErrorDescriptions.Catalog.ProductAlreadyExists);

        public static AppError CategoryAlreadyExists(string categoryName) =>
            new(AppErrorCodes.Catalog.CategoryAlreadyExists,
                $"Категорія з назвою '{categoryName}'. " + AppErrorDescriptions.Catalog.CategoryAlreadyExists);
    }
    public static class Order
    {
        public static AppError NotFound(int orderId) =>
            new(AppErrorCodes.Order.NotFound,
                $"Замовлення з ID {orderId} " + AppErrorDescriptions.Order.NotFound);

        public static AppError AlreadyExists(int orderId) =>
            new(AppErrorCodes.Order.AlreadyExists,
                $"Замовлення з ID {orderId} " + AppErrorDescriptions.Order.AlreadyExists);

        public static AppError ProcessingError(string details) =>
            new AppError(AppErrorCodes.Order.ProcessingError,
                AppErrorDescriptions.Order.ProcessingError)
                .WithDetails(details);

        public static AppError CancellationError(string details) =>
            new AppError(AppErrorCodes.Order.CancellationError,
                AppErrorDescriptions.Order.CancellationError)
                .WithDetails(details);
    }
    public static class Storage
    {
        public static AppError FileUploadFailed(string fileName) =>
            new(AppErrorCodes.Storage.FileUploadFailed,
                AppErrorDescriptions.Storage.FileUploadFailed + $"{fileName}");

        public static AppError FileNotFound(string fileId) =>
            new(AppErrorCodes.Storage.FileNotFound,
                $"По ID {fileId} " + AppErrorDescriptions.Storage.FileNotFound);
    }
    public static class Service
    {
        public static AppError ExternalServiceError(string serviceName, string details) =>
            new AppError(AppErrorCodes.Service.ExternalServiceError,
                string.Format(AppErrorDescriptions.Service.ExternalServiceError + serviceName))
                .WithDetails(details);
    }
    public static class Cache
    {
        public static AppError CacheMiss(string key) =>
            new(AppErrorCodes.Cache.Miss,
                $"Кеш не знайдено для ключа '{key}' " + AppErrorDescriptions.Cache.Miss);

        public static AppError CacheInvalidationFailed(string key) =>
            new(AppErrorCodes.Cache.InvalidationFailed,
                $"Не вдалося видалити кеш для ключа '{key}' " + AppErrorDescriptions.Cache.InvalidationFailed);
    }

}

