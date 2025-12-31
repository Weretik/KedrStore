namespace Catalog.Application.Features.Import.Queries.ImportCatalogFromXml;

public class ImportRootDto
{
    [JsonProperty("products")]
    public ImportCatalogItemsDto CatalogItems { get; set; } = null!;
}
