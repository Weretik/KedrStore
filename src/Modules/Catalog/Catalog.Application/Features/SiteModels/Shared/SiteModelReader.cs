using Catalog.Contracts.SiteModels;
using Catalog.Domain.Groups;
using Catalog.Application.Features.SiteModels.Shared.State;

namespace Catalog.Application.Features.SiteModels.Shared;

/// <summary>Reads one model as the admin shows it; <c>null</c> when it does not exist.</summary>
public sealed class SiteModelReader(SiteModelStateReader stateReader)
{
    public async Task<SiteModelDto?> ReadAsync(int modelId, CancellationToken cancellationToken)
    {
        var id = ProductGroupId.Create(modelId);
        var state = await stateReader.ReadModelsAsync([id], cancellationToken);
        return state.Models.ContainsKey(id) ? SiteModelMapper.ToDetail(state, id) : null;
    }
}
