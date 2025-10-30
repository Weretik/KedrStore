namespace Application.Catalog.ImportCatalogFromXml;

public sealed record ImportCatalogSummary(
    int CategoriesCreated,
    int ProductsCreated,
    int CategoriesUpdated,
    int ProductsUpdated,
    int CategoriesDeleted,
    int ProductsDeleted);
