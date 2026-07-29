using Sales.Application.Integrations.OneC.DTOs;

namespace Sales.Application.Integrations.OneC.Contracts;

public interface ISalesOneCReadClient
{
    Task<IReadOnlyList<OneCCounterpartyDto>> GetCounterpartiesAsync(CancellationToken cancellationToken);
    Task<IReadOnlyList<OneCCounterpartyCategoryPriceTypeDto>> GetCounterpartyCategoryPriceTypesAsync(CancellationToken cancellationToken);
}
