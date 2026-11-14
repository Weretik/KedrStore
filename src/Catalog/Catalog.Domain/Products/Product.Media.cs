using Catalog.Domain.Attributes;
using Catalog.Domain.Groups;
using Catalog.Domain.Products.Media;
using Catalog.Domain;
using Catalog.Domain.Attributes;
using Catalog.Domain.Categories;
using Catalog.Domain.Categories.Content;
using Catalog.Domain.Groups;
using Catalog.Domain.Products;
using Catalog.Domain.Products.Media;
using Catalog.Domain.Products.Prices;
using Catalog.Domain.Attributes;
using Catalog.Domain.Categories;
using Catalog.Domain.Groups;
using Catalog.Domain.PriceTypes;
using Catalog.Domain.Products;
using Catalog.Domain.Products.Media;
using Catalog.Domain.Slugs;

namespace Catalog.Domain.Products;

/// <summary>
/// Photos (0–20, in order) and the scheme (0–1) linked from the media library (R-30). <see cref="Photo"/> and
/// <see cref="Sсheme"/> are rewritten from the links on every change.
/// </summary>
public partial class Product
{
    public const int MaximumPhotos = 20;

    private readonly List<ProductMediaLink> _mediaLinks = [];

    public IReadOnlyList<ProductMediaLink> MediaLinks => _mediaLinks;

    public IEnumerable<ProductMediaLink> Photos => _mediaLinks
        .Where(link => link.Kind == ProductMediaKind.Photo)
        .OrderBy(link => link.Position);

    public ProductMediaLink? Scheme => _mediaLinks.FirstOrDefault(link => link.Kind == ProductMediaKind.Scheme);

    /// <summary>
    /// Replaces the whole set: photos in the given order and an optional scheme (R-30). Links that stay keep
    /// their row; the cache is rewritten. Returns whether anything changed.
    /// </summary>
    public bool SetMedia(IReadOnlyList<ProductMediaFile> photos, ProductMediaFile? scheme, DateTimeOffset now)
    {
        ArgumentNullException.ThrowIfNull(photos);
        if (photos.Count > MaximumPhotos)
            throw new DomainException(ProductMediaErrors.TooManyPhotos(photos.Count, MaximumPhotos));

        var wanted = photos
            .Select((file, index) => (File: file, Kind: ProductMediaKind.Photo, Position: index))
            .ToList();
        if (scheme is not null)
            wanted.Add((scheme, ProductMediaKind.Scheme, 0));

        var duplicate = wanted.GroupBy(item => item.File.Id).FirstOrDefault(group => group.Count() > 1);
        if (duplicate is not null)
            throw new DomainException(ProductMediaErrors.DuplicateFile(duplicate.Key));

        var wantedIds = wanted.Select(item => item.File.Id).ToHashSet();
        var changed = _mediaLinks.RemoveAll(link => !wantedIds.Contains(link.MediaFileId)) > 0;

        foreach (var (file, kind, position) in wanted)
        {
            var existing = _mediaLinks.FirstOrDefault(link => link.MediaFileId == file.Id);
            if (existing is null)
            {
                _mediaLinks.Add(ProductMediaLink.Create(file, kind, position));
                changed = true;
                continue;
            }

            changed |= existing.Place(kind, position);
            changed |= existing.SetUrl(file.Url);
        }

        return CompleteMediaChange(changed, now);
    }

    /// <summary>Links <paramref name="file"/> as the only photo when the product has no photo yet (R-33).</summary>
    public bool LinkPhotoIfNone(ProductMediaFile file, DateTimeOffset now)
        => LinkIfNone(file, ProductMediaKind.Photo, now);

    /// <summary>Links <paramref name="file"/> as the scheme when the product has no scheme yet (R-33).</summary>
    public bool LinkSchemeIfNone(ProductMediaFile file, DateTimeOffset now)
        => LinkIfNone(file, ProductMediaKind.Scheme, now);

    /// <summary>
    /// Rereads link URLs from <paramref name="currentUrls"/> (file ID → URL), drops links to files that no longer
    /// exist, and rewrites the cache (R-34). Returns whether anything changed.
    /// </summary>
    public bool RefreshMedia(IReadOnlyDictionary<long, string> currentUrls, DateTimeOffset now)
    {
        var changed = _mediaLinks.RemoveAll(link => !currentUrls.ContainsKey(link.MediaFileId)) > 0;
        foreach (var link in _mediaLinks)
            changed |= link.SetUrl(currentUrls[link.MediaFileId]);

        return CompleteMediaChange(changed, now);
    }

    /// <summary>Points the link to a replaced file at its new URL (R-35).</summary>
    public bool UpdateMediaUrl(long fileId, string url, DateTimeOffset now)
    {
        var link = _mediaLinks.FirstOrDefault(item => item.MediaFileId == fileId);
        return CompleteMediaChange(link is not null && link.SetUrl(url), now);
    }

    /// <summary>Drops the link to a deleted file; the next photo moves to first place (R-35).</summary>
    public bool RemoveMediaFile(long fileId, DateTimeOffset now)
        => CompleteMediaChange(_mediaLinks.RemoveAll(link => link.MediaFileId == fileId) > 0, now);

    private bool LinkIfNone(ProductMediaFile file, ProductMediaKind kind, DateTimeOffset now)
    {
        if (_mediaLinks.Any(link => link.Kind == kind || link.MediaFileId == file.Id))
            return false;

        _mediaLinks.Add(ProductMediaLink.Create(file, kind, 0));
        return CompleteMediaChange(true, now);
    }

    /// <summary>Renumbers photos from 0 and rewrites the cache; moves <c>UpdatedAt</c> only on a change.</summary>
    private bool CompleteMediaChange(bool linksChanged, DateTimeOffset now)
    {
        var position = 0;
        foreach (var photo in Photos.ToList())
            linksChanged |= photo.Place(ProductMediaKind.Photo, position++);

        var photo0 = Photos.FirstOrDefault()?.Url ?? string.Empty;
        var scheme = Scheme?.Url ?? string.Empty;
        var cacheChanged = !string.Equals(Photo, photo0, StringComparison.Ordinal) ||
                           !string.Equals(Sсheme, scheme, StringComparison.Ordinal);
        Photo = photo0;
        Sсheme = scheme;

        if (!linksChanged && !cacheChanged)
            return false;

        MarkAsUpdated(now);
        return true;
    }
}
