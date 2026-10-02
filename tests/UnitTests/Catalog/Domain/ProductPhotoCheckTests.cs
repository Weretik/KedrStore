using BuildingBlocks.Domain.Exceptions;
using Catalog.Domain.Entities;
using Catalog.Domain.Enums;
using Catalog.Domain.ValueObjects;

namespace UnitTests.Catalog.Domain;

public sealed class ProductPhotoCheckTests
{
    [Fact]
    public void CreateUnknown_StoresCurrentUrlWithoutCompletion()
    {
        var check = ProductPhotoCheck.CreateUnknown(
            ProductId.From(7),
            " https://images-kedr.cdn.express/products/7.jpg ");

        Assert.Equal(7, check.Id.Value);
        Assert.Equal("https://images-kedr.cdn.express/products/7.jpg", check.PhotoUrl);
        Assert.Equal(ProductPhotoStatus.Unknown, check.Status);
        Assert.Null(check.CheckedAtUtc);
        Assert.Null(check.HttpStatusCode);
        Assert.Null(check.DiagnosticCode);
        Assert.Null(check.DiagnosticMessage);
    }

    [Fact]
    public void ResetForUrl_ChangedUrlClearsPreviousResult()
    {
        var check = CreateCheck();
        check.RecordMissing(check.PhotoUrl, DateTimeOffset.UtcNow, 404);

        var changed = check.ResetForUrl("https://images-kedr.cdn.express/products/7-v2.jpg");

        Assert.True(changed);
        Assert.Equal(ProductPhotoStatus.Unknown, check.Status);
        Assert.Null(check.CheckedAtUtc);
        Assert.Null(check.HttpStatusCode);
        Assert.Null(check.DiagnosticCode);
    }

    [Fact]
    public void ResetForUrl_UnchangedUrlPreservesResult()
    {
        var check = CreateCheck();
        check.RecordMissing(check.PhotoUrl, DateTimeOffset.UtcNow, 404);

        var changed = check.ResetForUrl(check.PhotoUrl);

        Assert.False(changed);
        Assert.Equal(ProductPhotoStatus.Missing, check.Status);
    }

    [Fact]
    public void RecordAvailable_StoresSuccessfulTerminalState()
    {
        var check = CreateCheck();
        var now = DateTimeOffset.UtcNow;

        check.RecordAvailable(check.PhotoUrl, now, 200);

        Assert.Equal(ProductPhotoStatus.Available, check.Status);
        Assert.Equal(now, check.CheckedAtUtc);
        Assert.Equal(200, check.HttpStatusCode);
        Assert.Null(check.DiagnosticCode);
    }

    [Theory]
    [InlineData(404)]
    [InlineData(410)]
    public void RecordMissing_AcceptsConfirmedMissingStatuses(int statusCode)
    {
        var check = CreateCheck();

        check.RecordMissing(check.PhotoUrl, DateTimeOffset.UtcNow, statusCode);

        Assert.Equal(ProductPhotoStatus.Missing, check.Status);
        Assert.Equal(statusCode, check.HttpStatusCode);
        Assert.Equal("http_missing", check.DiagnosticCode);
    }

    [Fact]
    public void RecordInvalidContentType_StoresBoundedDiagnostic()
    {
        var check = CreateCheck();

        check.RecordInvalidContentType(check.PhotoUrl, DateTimeOffset.UtcNow, 200, "invalid_content_type", " text/html ");

        Assert.Equal(ProductPhotoStatus.InvalidContentType, check.Status);
        Assert.Equal("invalid_content_type", check.DiagnosticCode);
        Assert.Equal("text/html", check.DiagnosticMessage);
    }

    [Fact]
    public void RecordCheckFailed_AllowsTransportFailureWithoutHttpStatus()
    {
        var check = CreateCheck();

        check.RecordCheckFailed(check.PhotoUrl, DateTimeOffset.UtcNow, null, "timeout", "Request timed out");

        Assert.Equal(ProductPhotoStatus.CheckFailed, check.Status);
        Assert.Null(check.HttpStatusCode);
        Assert.Equal("timeout", check.DiagnosticCode);
    }

    [Fact]
    public void RecordResult_RejectsStaleUrl()
    {
        var check = CreateCheck();

        var exception = Assert.Throws<DomainException>(() =>
            check.RecordAvailable("https://images-kedr.cdn.express/products/other.jpg", DateTimeOffset.UtcNow, 200));

        Assert.Equal("Catalog.ProductPhotoCheck.Result.Stale", exception.Error.Code);
    }

    [Theory]
    [InlineData("")]
    [InlineData("http://images-kedr.cdn.express/products/7.jpg")]
    [InlineData("not-a-url")]
    public void CreateUnknown_RejectsInvalidPhotoUrl(string url)
    {
        var exception = Assert.Throws<DomainException>(() =>
            ProductPhotoCheck.CreateUnknown(ProductId.From(7), url));

        Assert.Equal("Catalog.ProductPhotoCheck.PhotoUrl.Invalid", exception.Error.Code);
    }

    private static ProductPhotoCheck CreateCheck()
        => ProductPhotoCheck.CreateUnknown(
            ProductId.From(7),
            "https://images-kedr.cdn.express/products/7.jpg");
}
