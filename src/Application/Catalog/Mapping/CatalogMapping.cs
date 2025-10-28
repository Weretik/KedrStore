namespace Application.Catalog.Mapping;

public static class CatalogMapping
{
    public static void Register(TypeAdapterConfig config, TimeProvider time)
    {
        ArgumentNullException.ThrowIfNull(config);
        ArgumentNullException.ThrowIfNull(time);

        config.NewConfig<ProductDto, Product>()
            .ConstructUsing(productDto => Product.Create(
                    ProductId.From(productDto.Id),
                    productDto.Name,
                    ProductCategoryId.From(productDto.CategoryId),
                    ProductType.FromValue(productDto.ProductTypeId),
                    productDto.Photo,
                    time.GetUtcNow(),
                    productDto.Stock))
            .AfterMapping((productDto, product) =>
            {
                foreach (var productPriceDto in productDto.Prices)
                {
                    var priceType = PriceType.FromName(productPriceDto.PriceType, ignoreCase: true);
                    product.UpsertPrice(priceType, productPriceDto.Amount, productPriceDto.CurrencyIso);
                }
            });

        config.NewConfig<Product, ProductDto>()
            .Map(productDto => productDto.Id, product => product.Id.Value)
            .Map(productDto => productDto.Name, product => product.Name)
            .Map(productDto => productDto.CategoryId, product => product.CategoryId.Value)
            .Map(productDto => productDto.ProductTypeId, product => product.ProductType.Value)
            .Map(productDto => productDto.Photo, product => product.Photo)
            .Map(productDto => productDto.Stock, product => product.Stock)
            .Map(productDto => productDto.Prices, product => product.Prices.Adapt<IReadOnlyList<ProductPriceDto>>());

        config.NewConfig<ProductPrice, ProductPriceDto>()
            .Map(productPriceDto => productPriceDto.PriceType, productPrice => productPrice.PriceType.Name)
            .Map(productPriceDto => productPriceDto.Amount, productPrice => productPrice.Price.Amount)
            .Map(productPriceDto => productPriceDto.CurrencyIso, productPrice => productPrice.Price.Currency.Code);

        config.NewConfig<ProductCategory, CategoryTreeDto>()
            .Map(categoryDto => categoryDto.Id, productCategory => productCategory.Id.Value)
            .Map(categoryDto => categoryDto.Name, productCategory => productCategory.Name)
            .Map(categoryDto => categoryDto.Children, _ => new List<CategoryTreeDto>());
    }
}
