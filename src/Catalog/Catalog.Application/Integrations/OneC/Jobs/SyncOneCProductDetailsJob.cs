using System.Text;
using Catalog.Application.Contracts.Persistence;
using Catalog.Application.Integrations.OneC.Contracts;
using Catalog.Application.Integrations.OneC.DTOs;
using Catalog.Application.Integrations.OneC.Mappers;
using Catalog.Application.Integrations.OneC.Specifications;
using Catalog.Domain.Entities;
using Catalog.Domain.ValueObjects;

namespace Catalog.Application.Integrations.OneC.Jobs;

public sealed class SyncOneCProductDetailsJob(
    IOneCClient oneC,
    ICatalogRepository<Product> productRepo,
    ICatalogRepository<ProductTranslation> translationRepo,
    ICatalogRepository<ProductCategory> categoryRepo,
    ILogger<SyncOneCPricesJob> logger)
{
    private const string RuLanguage = "ru";
    private const string TranslationFileRelativePath = "Translation/Imports/product-translations.ru.csv";

    public async Task RunAsync(string rootCategoryId, CancellationToken cancellationToken)
    {
        logger.LogInformation("[DEBUG_LOG] SyncOneCProductDetailsJob started for {Root}", rootCategoryId);

        var productsOneC = await oneC.GetProductDetailsAsync(rootCategoryId, cancellationToken);
        logger.LogInformation("[DEBUG_LOG] Received {Count} products from 1C for root {Root}", productsOneC.Count, rootCategoryId);

        if (productsOneC.Count == 0)
            return;

        var rows = await categoryRepo.ListAsync(new CategoryIdSlugMapSpec(), cancellationToken);
        var categoryNameDictionary = rows.ToDictionary(x => x.CategoryName, x => x.Id.Value);

        var products = CatalogMapper.MapProduct(productsOneC, categoryNameDictionary, rootCategoryId);

        await DeleteMissingAsync(products, rootCategoryId, cancellationToken);
        var syncedProductIds = await CreateOrUpsertProductsAsync(products, rootCategoryId, cancellationToken);
        await UpsertTranslationsForSyncedProductsAsync(syncedProductIds, cancellationToken);

        logger.LogInformation("SyncOneCProductDetailsJob finished for {Root}", rootCategoryId);
    }

    private async Task DeleteMissingAsync(IReadOnlyList<ProductRowOneCDto> productDtos, string rootCategoryOneCId, CancellationToken cancellationToken)
    {
        var importProductsIds = productDtos
            .Select(c => ProductId.From(c.Id))
            .Distinct()
            .ToArray();

        var spec = new ProductsByIdsSpec(importProductsIds, rootCategoryOneCId, true);
        await productRepo.DeleteRangeAsync(spec, cancellationToken);
    }

    private async Task<IReadOnlyCollection<ProductId>> CreateOrUpsertProductsAsync(IReadOnlyList<ProductRowOneCDto> productDtos, string rootCategoryId, CancellationToken cancellationToken)
    {
        var productIdsInBatch = productDtos.Select(x => ProductId.From(x.Id)).ToList();
        var existingProducts = await productRepo.ListAsync(new ProductsByIdsSpec(productIdsInBatch, rootCategoryId), cancellationToken);
        var existingDict = existingProducts.ToDictionary(x => x.Id, x => x);

        foreach (var item in productDtos)
        {
            var productId = ProductId.From(item.Id);

            if (!existingDict.TryGetValue(productId, out var existing))
            {
                var product = Product.Create(
                    id: productId,
                    productTypeIdOneC: item.ProductTypeIdOneC,
                    name: item.Name,
                    productSlug: item.ProducSlug,
                    categoryId: ProductCategoryId.From(item.CategoryId),
                    photo: item.Photo,
                    scheme: item.Scheme,
                    stock: item.Stock,
                    qtyInPack: item.QuantityInPack,
                    isNew: item.IsNew,
                    isSale: item.IsSale,
                    createdDate: DateTimeOffset.UtcNow
                );
                await productRepo.AddAsync(product, cancellationToken);

                existingDict[productId] = product;
                logger.LogInformation("[DEBUG_LOG] Added new product: {Name} (ID: {Id})", product.Name, product.Id);
            }
            else
            {
                existing.Update(
                    name: item.Name,
                    productSlug: item.ProducSlug,
                    categoryId: ProductCategoryId.From(item.CategoryId),
                    photo: item.Photo,
                    scheme: item.Scheme,
                    qtyInPack: item.QuantityInPack,
                    updatedDate: DateTimeOffset.UtcNow);

                if (item.IsNew) existing.MarkAsNew();
                else existing.RemoveNew();

                if (item.IsSale) existing.MarkAsSale();
                else existing.RemoveSale();
            }
        }

        await productRepo.SaveChangesAsync(cancellationToken);
        return productIdsInBatch.Distinct().ToArray();
    }

    private async Task UpsertTranslationsForSyncedProductsAsync(
        IReadOnlyCollection<ProductId> syncedProductIds,
        CancellationToken cancellationToken)
    {
        if (syncedProductIds.Count == 0)
            return;

        var translationsById = ReadTranslationsFromCsv();

        var existingTranslations = await translationRepo.ListAsync(
            new ProductTranslationsByProductIdsAndLanguageSpec(syncedProductIds, RuLanguage),
            cancellationToken);

        var existingByProductId = existingTranslations.ToDictionary(x => x.ProductId.Value, x => x);
        var now = DateTimeOffset.UtcNow;

        var created = 0;
        var updated = 0;
        var deleted = 0;

        foreach (var productId in syncedProductIds)
        {
            var hasTranslation = translationsById.TryGetValue(productId.Value, out var nameRu) &&
                                 !string.IsNullOrWhiteSpace(nameRu);

            if (!hasTranslation)
            {
                if (existingByProductId.TryGetValue(productId.Value, out var existingToDelete) && !existingToDelete.IsDeleted)
                {
                    existingToDelete.MarkAsDeleted(now);
                    deleted++;
                }

                continue;
            }

            if (existingByProductId.TryGetValue(productId.Value, out var existing))
            {
                existing.Update(nameRu!, now);
                updated++;
                continue;
            }

            var createdTranslation = ProductTranslation.Create(productId, RuLanguage, nameRu!, now);
            await translationRepo.AddAsync(createdTranslation, cancellationToken);
            created++;
        }

        await translationRepo.SaveChangesAsync(cancellationToken);

        logger.LogInformation(
            "Product translations synced. Created: {Created}, Updated: {Updated}, Deleted: {Deleted}, TotalProducts: {TotalProducts}",
            created, updated, deleted, syncedProductIds.Count);
    }

    private Dictionary<int, string> ReadTranslationsFromCsv()
    {
        var filePath = ResolveTranslationFilePath();
        if (!File.Exists(filePath))
        {
            logger.LogWarning(
                "Product translations file was not found. Expected path: {FilePath}. Translations sync will be skipped for this run.",
                filePath);
            return new Dictionary<int, string>();
        }

        Encoding.RegisterProvider(CodePagesEncodingProvider.Instance);

        using var reader = new StreamReader(filePath, Encoding.UTF8, detectEncodingFromByteOrderMarks: true);
        _ = reader.ReadLine();

        var result = new Dictionary<int, string>();
        while (reader.ReadLine() is { } line)
        {
            if (string.IsNullOrWhiteSpace(line))
                continue;

            var fields = ParseCsvLine(line);
            if (fields.Count < 3)
                continue;

            if (!int.TryParse(fields[0].Trim(), out var productId) || productId <= 0)
                continue;

            var ruName = fields[2].Trim();
            if (string.IsNullOrWhiteSpace(ruName))
                continue;

            result[productId] = ruName;
        }

        return result;
    }

    private static string ResolveTranslationFilePath()
    {
        return Path.Combine(AppContext.BaseDirectory, TranslationFileRelativePath);
    }

    private static List<string> ParseCsvLine(string line)
    {
        var fields = new List<string>();
        var sb = new StringBuilder();
        var inQuotes = false;

        for (var i = 0; i < line.Length; i++)
        {
            var ch = line[i];

            if (ch == '"')
            {
                if (inQuotes && i + 1 < line.Length && line[i + 1] == '"')
                {
                    sb.Append('"');
                    i++;
                }
                else
                {
                    inQuotes = !inQuotes;
                }

                continue;
            }

            if (ch == ',' && !inQuotes)
            {
                fields.Add(sb.ToString());
                sb.Clear();
                continue;
            }

            sb.Append(ch);
        }

        fields.Add(sb.ToString());
        return fields;
    }
}
