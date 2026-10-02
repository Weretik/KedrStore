using Catalog.Domain.Enums;
using Catalog.Domain.Errors;
using Catalog.Domain.ValueObjects;

namespace Catalog.Domain.Entities;

public sealed class ProductPhotoCheck : BaseEntity<ProductId>, IAggregateRoot
{
    public const int PhotoUrlMaxLength = 1000;
    public const int DiagnosticCodeMaxLength = 100;
    public const int DiagnosticMessageMaxLength = 500;

    public string PhotoUrl { get; private set; } = null!;
    public ProductPhotoStatus Status { get; private set; }
    public DateTimeOffset? CheckedAtUtc { get; private set; }
    public int? HttpStatusCode { get; private set; }
    public string? DiagnosticCode { get; private set; }
    public string? DiagnosticMessage { get; private set; }

    private ProductPhotoCheck() { }

    private ProductPhotoCheck(ProductId productId, string photoUrl)
    {
        Id = productId;
        SetUnknown(photoUrl);
    }

    public static ProductPhotoCheck CreateUnknown(ProductId productId, string photoUrl)
        => new(productId, photoUrl);

    public bool ResetForUrl(string photoUrl)
    {
        var normalized = NormalizePhotoUrl(photoUrl);
        if (string.Equals(PhotoUrl, normalized, StringComparison.Ordinal))
            return false;

        SetUnknown(normalized);
        return true;
    }

    public void RecordAvailable(string photoUrl, DateTimeOffset checkedAtUtc, int httpStatusCode)
    {
        EnsureCurrentUrl(photoUrl);
        EnsureCheckedAt(checkedAtUtc);
        if (httpStatusCode is < 200 or > 299)
            throw new DomainException(ProductPhotoCheckErrors.AvailableStatusInvalid(httpStatusCode));

        SetTerminal(ProductPhotoStatus.Available, checkedAtUtc, httpStatusCode, null, null);
    }

    public void RecordMissing(string photoUrl, DateTimeOffset checkedAtUtc, int httpStatusCode)
    {
        EnsureCurrentUrl(photoUrl);
        EnsureCheckedAt(checkedAtUtc);
        if (httpStatusCode is not (404 or 410))
            throw new DomainException(ProductPhotoCheckErrors.MissingStatusInvalid(httpStatusCode));

        SetTerminal(ProductPhotoStatus.Missing, checkedAtUtc, httpStatusCode, "http_missing", null);
    }

    public void RecordInvalidContentType(
        string photoUrl,
        DateTimeOffset checkedAtUtc,
        int httpStatusCode,
        string diagnosticCode,
        string? diagnosticMessage)
    {
        EnsureCurrentUrl(photoUrl);
        EnsureCheckedAt(checkedAtUtc);
        if (httpStatusCode is < 200 or > 299)
            throw new DomainException(ProductPhotoCheckErrors.AvailableStatusInvalid(httpStatusCode));

        SetTerminal(
            ProductPhotoStatus.InvalidContentType,
            checkedAtUtc,
            httpStatusCode,
            NormalizeDiagnosticCode(diagnosticCode),
            NormalizeDiagnosticMessage(diagnosticMessage));
    }

    public void RecordCheckFailed(
        string photoUrl,
        DateTimeOffset checkedAtUtc,
        int? httpStatusCode,
        string diagnosticCode,
        string? diagnosticMessage)
    {
        EnsureCurrentUrl(photoUrl);
        EnsureCheckedAt(checkedAtUtc);
        if (httpStatusCode is not null)
            EnsureHttpStatus(httpStatusCode.Value);

        SetTerminal(
            ProductPhotoStatus.CheckFailed,
            checkedAtUtc,
            httpStatusCode,
            NormalizeDiagnosticCode(diagnosticCode),
            NormalizeDiagnosticMessage(diagnosticMessage));
    }

    private void SetUnknown(string photoUrl)
    {
        PhotoUrl = NormalizePhotoUrl(photoUrl);
        Status = ProductPhotoStatus.Unknown;
        CheckedAtUtc = null;
        HttpStatusCode = null;
        DiagnosticCode = null;
        DiagnosticMessage = null;
    }

    private void SetTerminal(
        ProductPhotoStatus status,
        DateTimeOffset checkedAtUtc,
        int? httpStatusCode,
        string? diagnosticCode,
        string? diagnosticMessage)
    {
        Status = status;
        CheckedAtUtc = checkedAtUtc;
        HttpStatusCode = httpStatusCode;
        DiagnosticCode = diagnosticCode;
        DiagnosticMessage = diagnosticMessage;
    }

    private void EnsureCurrentUrl(string photoUrl)
    {
        var normalized = NormalizePhotoUrl(photoUrl);
        if (!string.Equals(PhotoUrl, normalized, StringComparison.Ordinal))
            throw new DomainException(ProductPhotoCheckErrors.StaleResult());
    }

    private static string NormalizePhotoUrl(string photoUrl)
    {
        var normalized = photoUrl?.Trim() ?? string.Empty;
        if (normalized.Length > PhotoUrlMaxLength)
            throw new DomainException(ProductPhotoCheckErrors.PhotoUrlTooLong(normalized.Length));

        if (!Uri.TryCreate(normalized, UriKind.Absolute, out var uri) ||
            !string.Equals(uri.Scheme, Uri.UriSchemeHttps, StringComparison.OrdinalIgnoreCase))
        {
            throw new DomainException(ProductPhotoCheckErrors.PhotoUrlInvalid());
        }

        return normalized;
    }

    private static void EnsureCheckedAt(DateTimeOffset checkedAtUtc)
    {
        if (checkedAtUtc == default)
            throw new DomainException(ProductPhotoCheckErrors.CheckedAtRequired());
    }

    private static void EnsureHttpStatus(int httpStatusCode)
    {
        if (httpStatusCode is < 100 or > 599)
            throw new DomainException(ProductPhotoCheckErrors.HttpStatusInvalid(httpStatusCode));
    }

    private static string NormalizeDiagnosticCode(string diagnosticCode)
    {
        if (string.IsNullOrWhiteSpace(diagnosticCode))
            throw new DomainException(ProductPhotoCheckErrors.DiagnosticCodeRequired());

        var normalized = diagnosticCode.Trim();
        if (normalized.Length > DiagnosticCodeMaxLength)
            throw new DomainException(ProductPhotoCheckErrors.DiagnosticCodeTooLong(normalized.Length));

        return normalized;
    }

    private static string? NormalizeDiagnosticMessage(string? diagnosticMessage)
    {
        if (string.IsNullOrWhiteSpace(diagnosticMessage))
            return null;

        var normalized = diagnosticMessage.Trim();
        if (normalized.Length > DiagnosticMessageMaxLength)
            throw new DomainException(ProductPhotoCheckErrors.DiagnosticMessageTooLong(normalized.Length));

        return normalized;
    }
}
