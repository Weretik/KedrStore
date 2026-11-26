using BuildingBlocks.Application.Caching;
using Catalog.Application.Features.SiteModels.Shared.State;
using Catalog.Contracts.RetailPrices;

namespace Catalog.Application.Features.RetailPrices.Commands.UnlockRetailPrices;

/// <summary>Gives every retail price locked by the owner's price file back to the 1C sync (spec catalog/010 RP-06).</summary>
public sealed record UnlockRetailPricesCommand : ICommand<Result<RetailPriceStatusDto>>, IInvalidatesReadCache
{
    /// <summary>The lock is not shown, but the state is dropped like for every price change.</summary>
    public IReadOnlyCollection<string> InvalidatedCacheTags => [SiteModelStateReader.Tag];
}
