namespace Application.Catalog.Commands.ImportCatalogFromXml;

public sealed class ImportCatalogFromXmlCommandHandler(
    ICatalogXmlParser parser,
    IMapper mapper,
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

    public async Task<(int productsDeleted, int categoriesDeleted)> DeleteMissingAsync(
        CatalogParseResult parsed, CancellationToken cancellationToken)
    {
        var importProductIds  = parsed.Products.Select(p => p.Id).ToHashSet();
        var importCategoryIds = parsed.Categories.Select(c => c.Id).ToHashSet();

        var prodSpec = new ProductsMissingInImportSpec(importProductIds);
        var catSpec  = new CategoryMissingInImportSpec(importCategoryIds);

        var productsDeleted   = await productRepo.DeleteRangeAsync(prodSpec, cancellationToken);
        var categoriesDeleted = await categoryRepo.DeleteRangeAsync(catSpec, cancellationToken);

        return (productsDeleted, categoriesDeleted);
    }

    public async Task<(int categoriesCreated, int categoriesUpdated)> CreateOrUpsertCategoriesAsync(
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

    public async Task<(int productsCreated, int productsUpdated)> CreateOrUpsertProductsAsync(
        CatalogParseResult parsed, CancellationToken cancellationToken)
    {
        int created = 0, updated = 0;
        var now = time.GetUtcNow();

        foreach (var productDto in parsed.Products.DistinctBy(productDto => productDto.Id))
        {
            var id = ProductId.From(productDto.Id);
            var existing = await productRead.GetByIdAsync(id, cancellationToken);

            if (existing is null)
            {
                var product = mapper.From(productDto).AdaptToType<Product>();
                await productRepo.AddAsync(product, cancellationToken);
                created++;
            }
            else
            {
                foreach (var productPriceDto in productDto.Prices)
                {
                    var priceType = PriceType.FromName(productPriceDto.PriceType, ignoreCase: true);
                    existing.UpsertPrice(priceType, productPriceDto.Amount, productPriceDto.CurrencyIso);
                }

                existing.Update(
                    productDto.Name,
                    ProductCategoryId.From(productDto.CategoryId),
                    ProductType.FromValue(productDto.ProductTypeId),
                    productDto.Photo,
                    now,
                    productDto.Stock
                );

                updated++;
            }
        }
        await productRepo.SaveChangesAsync(cancellationToken);
        return (created, updated);
    }
}
