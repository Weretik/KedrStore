namespace Application.Catalog.ImportCatalogFromXml;

public sealed record ImportCatalogFromXmlCommand(ImportCatalogFromXml Reuest) : ICommand<Result<ImportCatalogSummary>>;
