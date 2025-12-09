using Application.Catalog.Shared;

namespace Application.Catalog.ImportCatalogFromXml;

public static class ImportCatalogMapper
{
    public static CatalogMapperResult MapCatalog(ImportRootDto dto, int productTypeId)
    {
        var categories = GetBaseCategories(productTypeId);
        var products = new List<ProductDto>();

        foreach (var item in dto.CatalogItems.Product)
        {
            var id = TryGetInt(item.Id);
            var name = item.Name.Trim();
            var idR = TryGetInt(item.CategoryId);
            var nameR = item.CategoryName.Trim();
            var count = TryGetInt(item.Count);
            var prices = item.Prices;

            var skipWords = new[] { "KEDR", "Стенди", "Замiна KEDR, CLASS" };

            if (id <= 0
                || idR <= 0
                || string.IsNullOrWhiteSpace(name)
                || string.IsNullOrWhiteSpace(nameR)
                || prices == null
                || count < 0
                || skipWords.Any(w => nameR.Equals(w, StringComparison.OrdinalIgnoreCase)))
            {
                continue;
            }

            if (productTypeId == 2)
                nameR = NormalizeCategoryName(nameR);

            var parentMap = GetParentMap(productTypeId);
            parentMap.TryGetValue(idR, out var parentBaseId);

            if (productTypeId == 1 && parentBaseId == 0)
                parentBaseId = 103;

            var photo = $"https://cdn.jsdelivr.net/gh/inboxmakc-coder/kedr-images/furniture/{id}.jpg";
            string? scheme = $"https://cdn.jsdelivr.net/gh/inboxmakc-coder/kedr-images/scheme/s{id}.jpg";

            EnsureCategoryById(categories, idR, nameR, parentBaseId);

            var productPrice = new List<ProductPriceDto>();
            foreach (var price in prices)
            {
                productPrice.Add( new ProductPriceDto(
                    PriceType: price.Key,
                    Amount: TryGetDecimal(price.Value),
                    CurrencyIso: "UAH")
                );
            }

            products.Add(new ProductDto(
                Id: id,
                Name: name,
                CategoryId: idR,
                ProductTypeId: productTypeId,
                Photo: photo,
                Stock: count,
                Prices: productPrice,
                Scheme: scheme)
            );

        }

        return new CatalogMapperResult(categories, products) ;
    }

    static int TryGetInt(string? v) =>
        int.TryParse(v, out var x) ? x : 0;

    static decimal TryGetDecimal(string? v) =>
        decimal.TryParse(v, NumberStyles.Number, CultureInfo.InvariantCulture, out var x) ? x : 0m;

    static string NormalizeCategoryName(string name)
    {
        const string prefix = "Kedr ";
        name = name.Trim();

        return name.StartsWith(prefix, StringComparison.OrdinalIgnoreCase)
            ? name[prefix.Length..].TrimStart()
            : name;
    }

    static Dictionary<int, int> GetParentMap(int productTypeId) =>
        productTypeId switch
        {
            1 => new()
            {
                [19424] = 101, [8349] = 101, [8344] = 101, [26971] = 101,
                [7259] = 102, [7258] = 102, [27083] = 102, [19425] = 102, [8209] = 102
            },

            2 => new()
            {
                [4457] = 104, [6139] = 104, [1707] = 104,
                [5851] = 105, [1304] = 105, [2722] = 105, [2775] = 105, [2716] = 105,
                [5999] = 106, [5853] = 106, [26949] = 106, [6982] = 106,
                [5854] = 106, [5904] = 106, [5915] = 106, [6488] = 106,
                [6560] = 107, [2680] = 107, [26930] = 107, [26929] = 107,
                [5852] = 107, [4555] = 107, [27124] = 107,
                [5513] = 108, [5273] = 108, [2197] = 108, [6108] = 108, [2321] = 108
            },

            _ => new()
        };

    static List<ImportCategoryDto> GetBaseCategories(int productTypeId) =>
        productTypeId switch
        {
            1 => new()
            {
                new ImportCategoryDto(101, "Міжкімнатні двері", "n101"),
                new ImportCategoryDto(102, "Вхідні двері", "n102"),
                new ImportCategoryDto(103, "Інше", "n103")
            },

            2 => new()
            {
                new ImportCategoryDto(104, "Завіси", "n104"),
                new ImportCategoryDto(105, "Замки", "n105"),
                new ImportCategoryDto(106, "Ручки", "n106"),
                new ImportCategoryDto(107, "Циліндри", "n107"),
                new ImportCategoryDto(108, "Міжкімнатні механізми", "n108")
            },

            _ => throw new ArgumentOutOfRangeException(nameof(productTypeId))
        };

    static void EnsureCategoryById(List<ImportCategoryDto> list, int id, string name, int parentBaseId)
    {
        if (list.Any(x => x.Id == id))
            return;

        var parentPath = parentBaseId == 0
            ? ""
            : list.FirstOrDefault(x => x.Id == parentBaseId)?.Path ?? "";

        var path = string.IsNullOrWhiteSpace(parentPath) ? $"n{id}" : $"{parentPath}.n{id}";

        list.Add(new ImportCategoryDto(id, name, path));
    }
}
