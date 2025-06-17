namespace Application.Common.Results;

public static class AppErrors
{
    public static class System
    {
        public static AppError Validation(string message, string? details = null)
        {
            var error = new AppError(AppErrorTypes.System.Validation, message);
            return details is null ? error : error.WithDetails(details);
        }

        public static AppError NotFound(string entityName, string? id = null) =>
            new (AppErrorTypes.System.NotFound, $"{entityName} not found" + (id is not null ? $" (ID: {id})" : ""));

        public static AppError Conflict(string message) =>
            new(AppErrorTypes.System.Conflict, message);

        public static AppError Unexpected(string message) =>
            new(AppErrorTypes.System.Unexpected, message);

        public static AppError Persistence(string operation, string details) =>
            new AppError(AppErrorTypes.System.Persistence, $"Database operation '{operation}' failed")
                .WithDetails(details);
    }

    public static class Auth
    {
        public static AppError Unauthorized(string? reason = null) =>
            new(AppErrorTypes.Auth.Unauthorized, reason ?? "Access denied.");

        public static AppError Forbidden(string? reason = null) =>
            new(AppErrorTypes.Auth.Forbidden, reason ?? "You are not allowed to perform this action.");
    }

    public static class Domain
    {
        public static AppError InvalidCoupon(string code) =>
            new(AppErrorTypes.System.Validation, $"Invalid or expired coupon code: {code}");

        public static AppError InsufficientStock(string productName, int requested, int available) =>
            new(AppErrorTypes.Domain.Inventory, $"Insufficient stock for product '{productName}'. Requested: {requested}, Available: {available}");

        public static AppError InvalidPayment(string reason) =>
            new(AppErrorTypes.Domain.Payment, $"Payment failed: {reason}");

        public static AppError OrderNotProcessable(string orderId, string reason) =>
            new(AppErrorTypes.Domain.DomainViolation, $"Cannot process order {orderId}: {reason}");

        public static AppError IntegrationFailure(string service, string details) =>
            new AppError(AppErrorTypes.Domain.Integration, $"Integration with {service} failed")
                .WithDetails(details);
    }
}
