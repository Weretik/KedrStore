namespace Catalog.Application.Jobs.ProductPhotos;

public sealed class ProductPhotoCheckOptions
{
    public const string SectionName = "ProductPhotoCheck";

    public int BatchSize { get; init; } = 500;
    public int MaxConcurrency { get; init; } = 16;
    public int RequestTimeoutSeconds { get; init; } = 5;
    public int MaxRedirects { get; init; } = 3;
    public string[] AllowedHosts { get; init; } = ["images-kedr.cdn.express"];
}
