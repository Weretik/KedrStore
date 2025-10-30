using Application.Catalog.Shared;
using Domain.Catalog.Entities;
using Domain.Catalog.Enumerations;
using Domain.Catalog.ValueObjects;

namespace Application.Catalog.ImportCatalogFromXml;

public sealed class ImportCatalogFromXmlCommandHandler(
    ICatalogXmlParser parser,
    ICatalogReadRepository<ProductCategory> categoryRead,
    ICatalogRepository<ProductCategory> categoryRepo,
    ICatalogReadRepository<Product> productRead,
    ICatalogRepository<Product> productRepo,
    TimeProvider time)
    : ICommandHandler<ImportCatalogFromXmlCommand, Result<ImportCatalogSummary>>
{
    public async ValueTask<Result<ImportCatalogSummary>> Handle(
        ImportCatalogFromXmlCommand request,
        CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(request);

        var command = request.Request;
        if (command.Content.CanSeek) command.Content.Position = 0;

        var parsed = await parser.ParseAsync(command.Content, command.productType, cancellationToken);

        var (productsDeleted, categoriesDeleted) = await DeleteMissingAsync(parsed, cancellationToken);
        var (categoriesCreated, categoriesUpdated) = await CreateOrUpsertCategoriesAsync(parsed, cancellationToken);
        var (productsCreated, productsUpdated) = await CreateOrUpsertProductsAsync(parsed, cancellationToken);

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
        CatalogParseResult parsed, CancellationToken cancellationToken)
    {
        var importProductIds  = parsed.Products.Select(p => p.Id).ToHashSet();
        var importCategoryIds = parsed.Categories.Select(c => c.Id).ToHashSet();

        var productSpec = new ProductsMissingInImportSpec(importProductIds);
        var categorySpec  = new CategoryMissingInImportSpec(importCategoryIds);

        var productsDeleted   = await productRepo.DeleteRangeAsync(productSpec, cancellationToken);
        var categoriesDeleted = await categoryRepo.DeleteRangeAsync(categorySpec, cancellationToken);

        return (productsDeleted, categoriesDeleted);
    }

    private async Task<(int categoriesCreated, int categoriesUpdated)> CreateOrUpsertCategoriesAsync(
        CatalogParseResult parsed, CancellationToken cancellationToken)
    {
        int created = 0, updated = 0;

        foreach (var categoryDto in parsed.Categories.DistinctBy(categoryDto => categoryDto.Id))
        {
            var id = ProductCategoryId.From(categoryDto.Id);
            var existing = await categoryRead.GetByIdAsync(id, cancellationToken);

            if (existing is null)
            {
                var productCategory = ProductCategory.Create(
                    id,
                    categoryDto.Name,
                    new LTree(categoryDto.Path)
                );

                await categoryRepo.AddAsync(productCategory, cancellationToken);
                created++;
            }
            else
            {
                existing.Rename(categoryDto.Name);
                existing.Repath(new LTree(categoryDto.Path));
                updated++;
            }
        }
        await categoryRepo.SaveChangesAsync(cancellationToken);
        return (created, updated);
    }

    private async Task<(int productsCreated, int productsUpdated)> CreateOrUpsertProductsAsync(
        CatalogParseResult parsed, CancellationToken cancellationToken)
    {
        int created = 0, updated = 0;
        var dateNow = time.GetUtcNow();

        foreach (var productDto in parsed.Products.DistinctBy(productDto => productDto.Id))
        {
            var id = ProductId.From(productDto.Id);
            var categoryId = ProductCategoryId.From(productDto.CategoryId);
            var productType = ProductType.FromValue(productDto.ProductTypeId);

            var existing = await productRead.GetByIdAsync(id, cancellationToken);

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

        foreach (var dto in prices)
        {
            var priceType = PriceType.FromName(dto.PriceType, ignoreCase: true);
            var money = new Money(dto.Amount, dto.CurrencyIso);
            product.UpsertPrice(priceType, money);
        }
    }
}
