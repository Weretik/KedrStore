using Catalog.Contracts.SiteModels;
using Catalog.Domain.Attributes;
using Catalog.Domain.Categories;
using Catalog.Domain.Groups;
using Catalog.Domain.PriceTypes;
using Catalog.Domain.Products;
using Catalog.Domain.Products.Media;
using Catalog.Domain.Slugs;

namespace Catalog.Application.Features.SiteModels.Common;

/// <summary>Reads one model as the admin shows it; <c>null</c> when it does not exist.</summary>
public sealed class SiteModelReader(SiteModelStateReader stateReader)
{
    public async Task<SiteModelDto?> ReadAsync(int modelId, CancellationToken cancellationToken)
    {
        var state = await stateReader.ReadAsync(cancellationToken);
        var id = ProductGroupId.Create(modelId);
        return state.Models.ContainsKey(id) ? SiteModelMapper.ToDetail(state, id) : null;
    }
}
