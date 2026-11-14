using System.Text.RegularExpressions;
using Catalog.Application.Contracts.Media;
using Catalog.Application.Contracts.Persistence;
using Catalog.Application.Contracts.Projections;
using Catalog.Domain.Attributes;
using Catalog.Domain.Categories;
using Catalog.Domain.Groups;
using Catalog.Domain.PriceTypes;
using Catalog.Domain.Products;
using Catalog.Domain.Products.Media;
using Catalog.Domain.Slugs;

namespace Catalog.Application.Jobs.ProductMedia;

/// <summary>
/// Runs after the CDN sync: refreshes link URLs and the photo/scheme cache from the library (R-34), then links
/// <c>products/&lt;id&gt;.jpg</c> as the photo and <c>product-scheme/s&lt;id&gt;.jpg</c> as the scheme of every
/// active product that has none of that kind (R-33). <c>dryRun</c> reports and saves nothing.
/// </summary>
public sealed partial class LinkProductMediaJob(
    ICatalogRepository<Product> products,
    IProductMediaLibrary library,
    IProductListProjectionRebuilder projections,
    TimeProvider clock,
    ILogger<LinkProductMediaJob> logger)
{
    public const string PhotoDirectory = "products";
    public const string SchemeDirectory = "product-scheme";

    public async Task<LinkProductMediaReport> RunAsync(bool dryRun, CancellationToken cancellationToken)
    {
        var all = await products.ListAsync(new AllProductsWithMediaSpec(), cancellationToken);
        var now = clock.GetUtcNow();
        var changed = new HashSet<ProductId>();

        var linkedIds = all.SelectMany(product => product.MediaLinks).Select(link => link.MediaFileId)
            .Distinct().ToArray();
        var currentUrls = (await library.GetByIdsAsync(linkedIds, cancellationToken))
            .ToDictionary(file => file.Id, file => file.Url);

        var dropped = 0;
        foreach (var product in all)
        {
            var before = product.MediaLinks.Count;
            if (product.RefreshMedia(currentUrls, now))
                changed.Add(product.Id);
            dropped += before - product.MediaLinks.Count;
        }

        var photos = await FilesByProductIdAsync(PhotoDirectory, PhotoName(), cancellationToken);
        var schemes = await FilesByProductIdAsync(SchemeDirectory, SchemeName(), cancellationToken);
        int photosLinked = 0, schemesLinked = 0;
        foreach (var product in all.Where(product => !product.IsDeleted))
        {
            if (photos.TryGetValue(product.Id.Value, out var photo) &&
                product.LinkPhotoIfNone(new ProductMediaFile(photo.Id, photo.Url), now))
            {
                photosLinked++;
                changed.Add(product.Id);
            }

            if (schemes.TryGetValue(product.Id.Value, out var scheme) &&
                product.LinkSchemeIfNone(new ProductMediaFile(scheme.Id, scheme.Url), now))
            {
                schemesLinked++;
                changed.Add(product.Id);
            }
        }

        var report = new LinkProductMediaReport(
            dryRun,
            photosLinked,
            schemesLinked,
            dropped,
            changed.Count,
            all.Count(product => !product.IsDeleted && product.Photo.Length == 0));

        if (!dryRun && changed.Count > 0)
        {
            await products.SaveChangesAsync(cancellationToken);
            await projections.RefreshPhotosAsync(cancellationToken);
        }

        logger.LogInformation(
            "Product media link finished. DryRun: {DryRun}, photos linked: {PhotosLinked}, schemes linked: {SchemesLinked}, links dropped: {LinksDropped}, products changed: {ProductsChanged}, without photo: {WithoutPhoto}.",
            report.DryRun, report.PhotosLinked, report.SchemesLinked, report.LinksDropped, report.ProductsChanged,
            report.ProductsWithoutPhoto);
        return report;
    }

    /// <summary>
    /// Maps product IDs to files by name in one CDN directory. With several files for one product (for example
    /// <c>731.jpg</c> and <c>731.png</c>) the oldest library row wins, so reruns pick the same file.
    /// </summary>
    private async Task<Dictionary<int, ProductMediaLibraryFile>> FilesByProductIdAsync(string directory,
        Regex name, CancellationToken cancellationToken)
    {
        var files = await library.ListInDirectoryAsync(directory, cancellationToken);
        var result = new Dictionary<int, ProductMediaLibraryFile>();
        foreach (var file in files.OrderBy(file => file.Id))
        {
            var match = name.Match(file.StorageKey[(directory.Length + 1)..]);
            if (match.Success && int.TryParse(match.Groups["id"].Value, out var productId))
                result.TryAdd(productId, file);
        }

        return result;
    }

    [GeneratedRegex(@"^(?<id>\d{1,9})\.(jpg|jpeg|png|webp)$", RegexOptions.IgnoreCase | RegexOptions.CultureInvariant)]
    private static partial Regex PhotoName();

    [GeneratedRegex(@"^s(?<id>\d{1,9})\.(jpg|jpeg|png|webp)$", RegexOptions.IgnoreCase | RegexOptions.CultureInvariant)]
    private static partial Regex SchemeName();

    /// <summary>Every product, deleted ones too, tracked with its links.</summary>
    private sealed class AllProductsWithMediaSpec : Specification<Product>
    {
        public AllProductsWithMediaSpec() => Query.IgnoreQueryFilters();
    }
}
