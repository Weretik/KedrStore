namespace Application.Catalog.ImportCatalogFromXml;

public class ImportRootDto
{
    [JsonProperty("products")]
    public ImportCatalogItemsDto CatalogItems { get; set; } = null!;
}
