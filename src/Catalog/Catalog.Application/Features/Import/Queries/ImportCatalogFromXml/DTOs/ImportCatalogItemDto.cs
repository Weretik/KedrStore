namespace Catalog.Application.Features.Import.Queries.ImportCatalogFromXml;

public class ImportCatalogItemDto
{
    [JsonProperty("id")]
    public string Id { get; set; } = null!;

    [JsonProperty("name")]
    public string Name { get; set; } = null!;

    [JsonProperty("idR")]
    public string CategoryId { get; set; } = null!;

    [JsonProperty("nameR")]
    public string CategoryName { get; set; } = null!;

    [JsonProperty("prices")]
    public Dictionary<string, string> Prices { get; set; } = null!;

    [JsonProperty("count")]
    public string Count { get; set; } = null!;
    [JsonProperty("qty_in_pack")]
    public string qtyInPack { get; set; } = null!;
}
