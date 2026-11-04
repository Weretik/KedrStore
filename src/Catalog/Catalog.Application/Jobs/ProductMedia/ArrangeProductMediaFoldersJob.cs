using Catalog.Application.Contracts.Media;
using Catalog.Application.Contracts.Persistence;
using Catalog.Domain.Enums;

namespace Catalog.Application.Jobs.ProductMedia;

/// <summary>
/// Runs after linking (R-37): lays out the product photos in «Товари» and the schemes in «Схеми» as folders that
/// repeat the category tree, each file in the folder of its product's category. A file linked to several products
/// follows the product with the smallest ID. Only active products and categories count; files someone put outside
/// «Товари»/«Схеми» stay. <c>dryRun</c> reports and saves nothing.
/// </summary>
public sealed class ArrangeProductMediaFoldersJob(
    IReadCatalogDbContext db,
    IProductMediaLibrary library,
    ILogger<ArrangeProductMediaFoldersJob> logger)
{
    public async Task<ArrangeProductMediaFoldersReport> RunAsync(bool dryRun, CancellationToken cancellationToken)
    {
        var categories = await db.Categories.AsNoTracking()
            .Where(category => !category.IsDeleted)
            .ToListAsync(cancellationToken);
        var nodes = categories
            .Select(category => new ProductMediaFolderNode(category.Id.Value, category.Name, category.ParentId?.Value))
            .ToArray();
        var known = nodes.Select(node => node.CategoryId).ToHashSet();

        var links = await db.Products.AsNoTracking()
            .Where(product => !product.IsDeleted)
            .SelectMany(product => product.MediaLinks.Select(link => new
            {
                ProductId = product.Id.Value,
                CategoryId = product.CategoryId.Value,
                link.MediaFileId,
                link.Kind
            }))
            .ToListAsync(cancellationToken);

        var photos = await ArrangeAsync(LinkProductMediaJob.PhotoDirectory, ProductMediaKind.Photo);
        var schemes = await ArrangeAsync(LinkProductMediaJob.SchemeDirectory, ProductMediaKind.Scheme);
        var report = new ArrangeProductMediaFoldersReport(dryRun,
            photos.FoldersCreated + schemes.FoldersCreated,
            photos.FilesMoved + schemes.FilesMoved,
            photos.FilesKeptOutside + schemes.FilesKeptOutside);

        logger.LogInformation(
            "Product media folders finished. DryRun: {DryRun}, folders created: {FoldersCreated}, files moved: {FilesMoved}, kept outside: {KeptOutside}.",
            report.DryRun, report.FoldersCreated, report.FilesMoved, report.FilesKeptOutside);
        return report;

        Task<ProductMediaFolderResult> ArrangeAsync(string directory, ProductMediaKind kind)
        {
            var placements = links
                .Where(link => link.Kind == kind && known.Contains(link.CategoryId))
                .GroupBy(link => link.MediaFileId)
                .Select(group => group.MinBy(link => link.ProductId)!)
                .Select(link => new ProductMediaFolderPlacement(link.MediaFileId, link.CategoryId))
                .ToArray();
            return library.ArrangeFoldersAsync(directory, nodes, placements, dryRun, cancellationToken);
        }
    }
}

/// <summary>What one folder run did, or would do with <see cref="DryRun"/>.</summary>
public sealed record ArrangeProductMediaFoldersReport(bool DryRun, int FoldersCreated, int FilesMoved, int FilesKeptOutside);
