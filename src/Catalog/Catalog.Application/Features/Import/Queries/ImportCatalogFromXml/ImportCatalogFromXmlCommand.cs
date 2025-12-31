namespace Catalog.Application.Features.Import.Queries.ImportCatalogFromXml;

public sealed record ImportCatalogFromXmlCommand(global::Catalog.Application.Features.Import.Queries.ImportCatalogFromXml.ImportCatalogFromXml Request) : ICommand<Result<ImportCatalogSummary>>;
