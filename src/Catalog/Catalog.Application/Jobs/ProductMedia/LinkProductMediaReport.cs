namespace Catalog.Application.Jobs.ProductMedia;

/// <summary>What one link run did, or would do with <see cref="DryRun"/>.</summary>
public sealed record LinkProductMediaReport(
    bool DryRun,
    int PhotosLinked,
    int SchemesLinked,
    int LinksDropped,
    int ProductsChanged,
    int ProductsWithoutPhoto);
