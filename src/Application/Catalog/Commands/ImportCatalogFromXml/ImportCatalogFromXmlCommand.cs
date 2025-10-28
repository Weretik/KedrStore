namespace Application.Catalog.Commands.ImportCatalogFromXml;

public sealed record ImportCatalogFromXmlCommand(CatalogFromXml Request) : ICommand<Result<ImportCatalogSummary>>;
