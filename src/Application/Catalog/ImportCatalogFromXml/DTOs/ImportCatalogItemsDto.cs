namespace Application.Catalog.ImportCatalogFromXml;

public class ImportCatalogItemsDto
{
    [JsonProperty("product")]
    public List<ImportCatalogItemDto> Product { get; set; } = null!;
}
