namespace Application.Common.Results;

public sealed record AppError(string Code, string Message)
{
    public string? Details { get; private init; }

    public AppError WithDetails(string details) =>
        this with { Details = details };

    public override string ToString() =>
        $"{Code}: {Message}" + (Details is not null ? $" | Details: {Details}" : string.Empty);

    #region Common Errors

    public static AppError NotFound(string entityName, string? id = null) =>
        new(AppErrorTypes.NotFound, $"{entityName} not found" + (id is not null ? $" (ID: {id})" : ""));

    public static AppError Validation(string message) =>
        new(AppErrorTypes.Validation, message);

    public static AppError Unauthorized(string? reason = null) =>
        new(AppErrorTypes.Unauthorized, reason ?? "Access denied.");

    public static AppError Forbidden(string? reason = null) =>
        new(AppErrorTypes.Forbidden, reason ?? "You are not allowed to perform this action.");

    public static AppError Conflict(string message) =>
        new(AppErrorTypes.Conflict, message);

    public static AppError Unexpected(string message) =>
        new(AppErrorTypes.Unexpected, message);

    #endregion

    #region Domain Specific Errors

    public static AppError InsufficientStock(string productName, int requested, int available) =>
        new(AppErrorTypes.Inventory, $"Insufficient stock for product '{productName}'. " +
                                     $"Requested: {requested}, Available: {available}");

    public static AppError InvalidPayment(string reason) =>
        new(AppErrorTypes.Payment, $"Payment failed: {reason}");

    public static AppError InvalidCoupon(string code) =>
        new(AppErrorTypes.Validation, $"Invalid or expired coupon code: {code}");

    public static AppError OrderNotProcessable(string orderId, string reason) =>
        new(AppErrorTypes.DomainViolation, $"Cannot process order {orderId}: {reason}");

    public static AppError DatabaseOperation(string operation, string details) =>
        new(AppErrorTypes.Persistence, $"Database operation '{operation}' failed");

    public static AppError IntegrationFailure(string service, string details) =>
        new(AppErrorTypes.Integration, $"Integration with {service} failed");

    #endregion
}
