using Catalog.Domain.Enums;
using Catalog.Domain.Errors;
using Catalog.Domain.ValueObjects;

namespace Catalog.Domain.Entities;

/// <summary>
/// A link from a product to a library file, owned by <see cref="Product"/>. The file lives in Media and is known
/// only by its ID; <see cref="Url"/> is a copy for one-table reads, refreshed by the CDN sync run (R-34).
/// </summary>
public sealed class ProductMediaLink
{
    public const int UrlMaxLength = 1000;

    public long MediaFileId { get; private set; }
    public ProductMediaKind Kind { get; private set; }
    public int Position { get; private set; }
    public string Url { get; private set; } = null!;

    private ProductMediaLink() { }

    internal static ProductMediaLink Create(ProductMediaFile file, ProductMediaKind kind, int position)
    {
        if (file.Id <= 0)
            throw new DomainException(ProductMediaErrors.FileIdInvalid(file.Id));

        var link = new ProductMediaLink { MediaFileId = file.Id };
        link.Place(kind, position);
        link.SetUrl(file.Url);
        return link;
    }

    internal bool Place(ProductMediaKind kind, int position)
    {
        if (Kind == kind && Position == position)
            return false;

        Kind = kind;
        Position = position;
        return true;
    }

    internal bool SetUrl(string url)
    {
        var normalized = url?.Trim() ?? string.Empty;
        if (normalized.Length is 0 or > UrlMaxLength)
            throw new DomainException(ProductMediaErrors.UrlInvalid());
        if (string.Equals(Url, normalized, StringComparison.Ordinal))
            return false;

        Url = normalized;
        return true;
    }
}
