using Application.Catalog.Shared;
using Domain.Catalog.Entities;
using Domain.Catalog.Enumerations;
using Domain.Catalog.ValueObjects;
using Domain.Common.ValueObject;
using Newtonsoft.Json;

namespace Application.Catalog.ImportCatalogFromXml;

public sealed class ImportCatalogFromXmlCommandHandler(
    IXmlToJsonConvector toJsonConvector,
    TimeProvider time,
    ILogger<ImportCatalogFromXmlCommandHandler> logger,
    ICatalogRepository<ProductCategory> categoryRepo,
    ICatalogRepository<Product> productRepo)
    : ICommandHandler<ImportCatalogFromXmlCommand, Result<ImportCatalogSummary>>
{
    public async ValueTask<Result<ImportCatalogSummary>> Handle(ImportCatalogFromXmlCommand request, CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(request);
        var command = request.Request;

        CommandLog.ImportCatalogFromXml(
            logger,
            command.File.FileName,
            command.File.Length,
            command.ProductTypeId);

        // Copy the source file stream to memory
        // so that it can be read again during parsing (XmlReader consumes the stream once)
        await using var browserStream = command.File.OpenReadStream();
        await using var memoryStream = new MemoryStream();
        await browserStream.CopyToAsync(memoryStream, cancellationToken);

        memoryStream.Position = 0;

        var jsonStream = await toJsonConvector.CreateJsonStreamAsync(memoryStream, cancellationToken);

        using var reader = new StreamReader(jsonStream);
        string json = await reader.ReadToEndAsync(cancellationToken);

        ImportRootDto dto = JsonConvert.DeserializeObject<ImportRootDto>(json)
                            ?? throw new InvalidOperationException("Unable to deserialize XML preview DTO.");

        var parsed = ImportCatalogMapper.MapCatalog(dto, command.ProductTypeId);

        // Delete products and categories that are no longer present in the new import
        var (productsDeleted, categoriesDeleted) = await DeleteMissingAsync(
            parsed,
            command.ProductTypeId,
            cancellationToken
        );
        var (categoriesCreated, categoriesUpdated) = await CreateOrUpsertCategoriesAsync(
            parsed,
            command.ProductTypeId,
            cancellationToken
        );
        var (productsCreated, productsUpdated) = await CreateOrUpsertProductsAsync(
            parsed,
            command.ProductTypeId,
            cancellationToken
        );

        return new ImportCatalogSummary(
            CategoriesCreated: categoriesCreated,
            ProductsCreated: productsCreated,
            CategoriesUpdated: categoriesUpdated,
            ProductsUpdated: productsUpdated,
            CategoriesDeleted: categoriesDeleted,
            ProductsDeleted: productsDeleted
        );
    }

    private async Task<(int productsDeleted, int categoriesDeleted)> DeleteMissingAsync(
        CatalogMapperResult parsed, int productTypeId, CancellationToken cancellationToken)
    {
        var importProductIds  = parsed.Products
            .Select(p => ProductId.From(p.Id))
            .Distinct().ToArray();
        var importCategoryIds = parsed.Categories
            .Select(c => ProductCategoryId.From(c.Id))
            .Distinct().ToArray();

        var productSpec = new ProductsMissingInImportSpec(importProductIds, productTypeId);
        var categorySpec  = new CategoryMissingInImportSpec(importCategoryIds, productTypeId);

        var productsDeleted = await productRepo.DeleteRangeAsync(productSpec, cancellationToken);
        var categoriesDeleted = await categoryRepo.DeleteRangeAsync(categorySpec, cancellationToken);

        return (productsDeleted, categoriesDeleted);
    }

    private async Task<(int categoriesCreated, int categoriesUpdated)> CreateOrUpsertCategoriesAsync(
        CatalogMapperResult parsed, int productTypeId, CancellationToken cancellationToken)
    {
        int created = 0, updated = 0;

        foreach (var categoryDto in parsed.Categories.DistinctBy(categoryDto => categoryDto.Id))
        {
            var id = ProductCategoryId.From(categoryDto.Id);
            var existing = await categoryRepo.GetByIdAsync(id, cancellationToken);

            if (existing is null)
            {
                var productCategory = ProductCategory.Create(
                    id,
                    categoryDto.Name,
                    CategoryPath.From(categoryDto.Path),
                    ProductType.FromValue(productTypeId)
                );

                await categoryRepo.AddAsync(productCategory, cancellationToken);
                created++;
            }
            else
            {
                existing.Rename(categoryDto.Name);
                existing.Repath(CategoryPath.From(categoryDto.Path));
                updated++;
            }
        }
        await categoryRepo.SaveChangesAsync(cancellationToken);
        return (created, updated);
    }

    private async Task<(int productsCreated, int productsUpdated)> CreateOrUpsertProductsAsync(
        CatalogMapperResult parsed, int productTypeId, CancellationToken cancellationToken)
    {
        int created = 0, updated = 0;
        var dateNow = time.GetUtcNow();
        var productType = ProductType.FromValue(productTypeId);

        foreach (var productDto in parsed.Products.DistinctBy(productDto => productDto.Id))
        {
            var id = ProductId.From(productDto.Id);
            var categoryId = ProductCategoryId.From(productDto.CategoryId);

            var existing = await productRepo.GetByIdAsync(id, cancellationToken);

            if (existing is null)
            {
                var product = Product.Create(
                    id,
                    productDto.Name,
                    categoryId,
                    productType,
                    productDto.Photo,
                    dateNow,
                    productDto.Stock);
                // Overwrite or add prices (Upsert by price type)
                ApplyPrices(product, productDto.Prices);

                await productRepo.AddAsync(product, cancellationToken);
                created++;
            }
            else
            {
                existing.Update(
                    productDto.Name,
                    categoryId,
                    productType,
                    productDto.Photo,
                    dateNow,
                    productDto.Stock
                );
                // Overwrite or add prices (Upsert by price type)
                ApplyPrices(existing, productDto.Prices);
                updated++;
            }
        }
        await productRepo.SaveChangesAsync(cancellationToken);
        return (created, updated);
    }

    private static void ApplyPrices(Product product, IReadOnlyCollection<ProductPriceDto> prices)
    {
        if ( prices.Count == 0) return;

        var productPriceList = new List<ProductPrice>();

        foreach (var dto in prices)
        {
            var priceType = PriceType.FromName(dto.PriceType, ignoreCase: true);
            var money = new Money(dto.Amount, dto.CurrencyIso);
            productPriceList.Add(ProductPrice.Create(priceType, money));
        }
        product.ReplaceAllPrices(productPriceList);
    }
}
