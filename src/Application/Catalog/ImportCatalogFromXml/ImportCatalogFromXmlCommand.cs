namespace Application.Catalog.ImportCatalogFromXml;

public sealed record ImportCatalogFromXmlCommand(ImportCatalogFromXml Request) : ICommand<Result<ImportCatalogSummary>>;
