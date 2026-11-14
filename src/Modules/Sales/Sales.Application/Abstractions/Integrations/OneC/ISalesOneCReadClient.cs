using Sales.Application.Abstractions.Integrations.OneC;

namespace Sales.Application.Abstractions.Integrations.OneC;

public interface ISalesOneCReadClient
{
    Task<IReadOnlyList<OneCCounterpartyDto>> GetCounterpartiesAsync(CancellationToken cancellationToken);
    Task<IReadOnlyList<OneCCounterpartyCategoryPriceTypeDto>> GetCounterpartyCategoryPriceTypesAsync(CancellationToken cancellationToken);
}
