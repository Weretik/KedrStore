namespace Catalog.Domain.Errors;

public static class ProductPhotoCheckErrors
{
    public static CatalogDomainError PhotoUrlInvalid() =>
        new("Catalog.ProductPhotoCheck.PhotoUrl.Invalid", "Product photo URL must be an absolute HTTPS URL.");

    public static CatalogDomainError PhotoUrlTooLong(int length) =>
        new("Catalog.ProductPhotoCheck.PhotoUrl.TooLong", $"Product photo URL cannot exceed 1000 characters. Actual: {length}.");

    public static CatalogDomainError StaleResult() =>
        new("Catalog.ProductPhotoCheck.Result.Stale", "The photo check result does not belong to the current photo URL.");

    public static CatalogDomainError CheckedAtRequired() =>
        new("Catalog.ProductPhotoCheck.CheckedAt.Required", "A completed photo check requires a completion timestamp.");

    public static CatalogDomainError HttpStatusInvalid(int statusCode) =>
        new("Catalog.ProductPhotoCheck.HttpStatus.Invalid", $"HTTP status code must be between 100 and 599. Actual: {statusCode}.");

    public static CatalogDomainError AvailableStatusInvalid(int statusCode) =>
        new("Catalog.ProductPhotoCheck.Available.HttpStatusInvalid", $"Available photo status requires a 2xx HTTP status. Actual: {statusCode}.");

    public static CatalogDomainError MissingStatusInvalid(int statusCode) =>
        new("Catalog.ProductPhotoCheck.Missing.HttpStatusInvalid", $"Missing photo status requires HTTP 404 or 410. Actual: {statusCode}.");

    public static CatalogDomainError DiagnosticCodeRequired() =>
        new("Catalog.ProductPhotoCheck.DiagnosticCode.Required", "A diagnostic code is required for an unavailable photo result.");

    public static CatalogDomainError DiagnosticCodeTooLong(int length) =>
        new("Catalog.ProductPhotoCheck.DiagnosticCode.TooLong", $"Diagnostic code cannot exceed 100 characters. Actual: {length}.");

    public static CatalogDomainError DiagnosticMessageTooLong(int length) =>
        new("Catalog.ProductPhotoCheck.DiagnosticMessage.TooLong", $"Diagnostic message cannot exceed 500 characters. Actual: {length}.");
}
