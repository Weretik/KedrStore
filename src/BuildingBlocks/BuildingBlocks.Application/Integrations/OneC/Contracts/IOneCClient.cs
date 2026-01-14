using BuildingBlocks.Application.Integrations.OneC.DTOs;

namespace BuildingBlocks.Application.Integrations.OneC.Contracts;

public interface IOneCClient
{
    Task<IReadOnlyList<OneCCategoryDto>> GetCategoriesAsync(string rootCategoryId, CancellationToken cancellationToken);
    Task<IReadOnlyList<OneCProductDto>> GetProductDetailsAsync(string rootCategoryId, CancellationToken cancellationToken);
    Task<IReadOnlyList<OneCStockDto>> GetProductStocksAsync(string rootCategoryId, CancellationToken cancellationToken);
    Task<IReadOnlyList<OneCPriceDto>> GetProductPricesAsync(string rootCategoryId, CancellationToken cancellationToken);
    Task<IReadOnlyList<OneCPriceTypeDto>> GetPriceTypesAsync(CancellationToken cancellationToken);
}
