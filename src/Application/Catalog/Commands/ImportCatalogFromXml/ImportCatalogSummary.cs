namespace Application.Catalog.Commands.ImportCatalogFromXml;

public sealed record ImportCatalogSummary(
    int CategoriesCreated,
    int ProductsCreated,
    int Updated,
    int Skipped,
    int Error,
    int Deleted);
