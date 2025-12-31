namespace Catalog.Application.Features.Import.Queries.ImportCatalogFromXml;

public class ImportCatalogItemsDto
{
    [JsonProperty("product")]
    public List<ImportCatalogItemDto> Product { get; set; } = null!;
}
