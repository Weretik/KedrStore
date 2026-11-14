using Catalog.Application.Features.OneCSync.Source;
using Catalog.Application.Features.OneCSync.Source.Prices;

namespace Catalog.Application.Abstractions.Integrations;

public interface IOneCClient
{
    Task<IReadOnlyList<OneCCategoryDto>> GetCategoriesAsync(string rootCategoryId, CancellationToken cancellationToken);
    Task<IReadOnlyList<OneCProductDto>> GetProductDetailsAsync(string rootCategoryId, CancellationToken cancellationToken);
    Task<IReadOnlyList<OneCStockDto>> GetProductStocksAsync(string rootCategoryId, CancellationToken cancellationToken);
    Task<IReadOnlyList<OneCPriceDto>> GetProductPricesAsync(string rootCategoryId, CancellationToken cancellationToken);
    Task<IReadOnlyList<OneCPriceTypeDto>> GetPriceTypesAsync(CancellationToken cancellationToken);
}
