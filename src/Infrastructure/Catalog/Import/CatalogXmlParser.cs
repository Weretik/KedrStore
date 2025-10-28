namespace Infrastructure.Catalog.Import;

public class CatalogXmlParser(ILogger<CatalogXmlParser> logger) : ICatalogXmlParser
{
    public async Task<CatalogParseResult> ParseAsync(Stream xml, int productTypeId, CancellationToken cancellationToken = default)
    {
        logger.LogInformation("XML parse started");

        var settings = new XmlReaderSettings
        {
            DtdProcessing = DtdProcessing.Prohibit,
            XmlResolver = null,
            IgnoreComments = true,
            IgnoreWhitespace = true
        };

        var categories = GetBaseCategories(productTypeId);
        var products = new List<ProductDto>();

        using var xmlReader = XmlReader.Create(xml, settings);
        var xmlDocument = XDocument.Load(xmlReader);
        var root = xmlDocument.Root?.Elements("product") ?? [];

        foreach (var xmlElement in root)
        {
            cancellationToken.ThrowIfCancellationRequested();

            var id = TryInt(xmlElement.Element("id")?.Value);
            var name = (xmlElement.Element("name")?.Value ?? "").Trim();
            var idR = TryInt(xmlElement.Element("idR")?.Value);
            var nameR = NormalizeCategoryName((xmlElement.Element("nameR")?.Value ?? "").Trim(), productTypeId);
            var count = TryInt(xmlElement.Element("count")?.Value);

            var parentMap = GetParentMap(productTypeId);
            parentMap.TryGetValue(idR, out var parentBaseId);

            if (productTypeId == 1 && parentBaseId == 0)
                parentBaseId = 103;

            EnsureCategoryById(categories, idR, nameR, parentBaseId);

            var pricesNode = xmlElement.Element("prices");
            var prices = ReadPrices(pricesNode!);
            var photoLinks = $"https://raw.githubusercontent.com/Kedr-Class/images/refs/heads/main/furniture/{id}.jpg";

            products.Add(new ProductDto(
                Id: id,
                Name: name,
                CategoryId: idR,
                ProductTypeId: productTypeId,
                Photo: photoLinks,
                Stock: count,
                Prices: prices
            ));
        }

        logger.LogInformation("XML parse stopped");
        return await Task.FromResult(new CatalogParseResult(categories, products));
    }

    static List<CategoryDto> GetBaseCategories(int productTypeId)
    {
        return productTypeId switch
        {
            1 =>
            [
                new CategoryDto(101, "Міжкімнатні двері", "n101"),
                new CategoryDto(102, "Вхідні двері", "n102"),
                new CategoryDto(103, "Інше", "n103")
            ],
            2 =>
            [
                new CategoryDto(104, "Завіси", "n104"),
                new CategoryDto(105, "Замки", "n105"),
                new CategoryDto(106, "Ручки", "n106"),
                new CategoryDto(107, "Циліндри", "n107")
            ],
            _ => throw new ArgumentOutOfRangeException(nameof(productTypeId), productTypeId, null)
        };
    }

    static int TryInt(string? element)
        => int.TryParse(element, out var value) ? value : 0;

    static decimal TryDecimal(string? element)
        => decimal.TryParse(element, NumberStyles.Number, CultureInfo.InvariantCulture, out var value) ? value : 0m;

    static List<ProductPriceDto> ReadPrices(XElement pricesNode)
    {
        var result = new List<ProductPriceDto>();
        foreach (var price in pricesNode.Elements())
        {
            var priceType = price.Name.LocalName;
            if (!priceType.StartsWith("price_", StringComparison.OrdinalIgnoreCase)) continue;

            var dto = new ProductPriceDto(
                PriceType: priceType.Trim(),
                Amount: TryDecimal(price.Value),
                CurrencyIso: "UAH");

            result.Add(dto);
        }
        return result;
    }
    static string NormalizeCategoryName(string name, int productTypeId)
    {
        if (productTypeId == 1)
        {
            const string deletedString = "Kedr  ";

            var resultName = name.Trim();
            if (resultName.StartsWith(deletedString, StringComparison.OrdinalIgnoreCase))
            {
                resultName = resultName.Substring(deletedString.Length).TrimStart();
            }

            return resultName;
        }
        return name;
    }

    static Dictionary<int, int> GetParentMap(int productTypeId) => productTypeId switch
    {
        1 => new()
        {
            [19424] = 101, [8349] = 101, [8344] = 101, [26971] = 101,

            [7259] = 102, [7258] = 102, [27083] = 102, [19425] = 102, [8209] = 102,
        },
        2 => new()
        {
            [4457] = 104, [6139] = 104, [1707] = 104,

            [5851] = 105, [1304] = 105, [2722] = 105, [2775] = 105, [2716] = 105,

            [5999] = 106, [5853] = 106, [26949] = 106, [6982] = 106, [5854] = 106, [5904] = 106, [5915] = 106, [6488] = 106,

            [6560] = 107, [2680] = 107, [26930] = 107, [26929] = 107, [5852] = 107, [4555] = 107, [27124] = 107,
        },
        _ => new()
    };

    static void EnsureCategoryById(List<CategoryDto> list, int id, string name, int parentBaseId)
    {
        for (int i = 0; i < list.Count; i++)
            if (list[i].Id == id) return;

        var parentPath = parentBaseId == 0 ? "" : PathOfId(list, parentBaseId);
        var path = string.IsNullOrWhiteSpace(parentPath) ? $"n{id}" : $"{parentPath}.n{id}";
        list.Add(new CategoryDto(id, name, path));
    }

    static string PathOfId(List<CategoryDto> list, int id)
    {
        for (int i = 0; i < list.Count; i++)
            if (list[i].Id == id) return list[i].Path;
        return "";
    }
}
