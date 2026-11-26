using BuildingBlocks.Application.Caching;
using Catalog.Application.Features.SiteModels.Shared.State;
using Catalog.Contracts.RetailPrices;

namespace Catalog.Application.Features.RetailPrices.Commands.ImportRetailPrices;

/// <summary>An uploaded price file as the API received it.</summary>
public sealed record RetailPriceUpload(string FileName, long Length, Func<Stream> OpenReadStream);

/// <summary>
/// Sets the site's retail prices from the 1C price list exported to Excel, by product code (spec catalog/010 RP-02…RP-04);
/// with <paramref name="ProtectFromOneC"/> the 1C sync keeps its hands off the prices from the file.
/// </summary>
public sealed record ImportRetailPricesCommand(RetailPriceUpload File, bool ProtectFromOneC)
    : ICommand<Result<RetailPriceImportResultDto>>, IInvalidatesReadCache
{
    /// <summary>The site models state shows retail prices.</summary>
    public IReadOnlyCollection<string> InvalidatedCacheTags => [SiteModelStateReader.Tag];
}
