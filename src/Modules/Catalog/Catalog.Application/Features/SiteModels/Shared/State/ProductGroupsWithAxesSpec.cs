using Catalog.Domain.Groups;

namespace Catalog.Application.Features.SiteModels.Shared.State;

/// <summary>The models with their axes, for the commands that change the axes through the model.</summary>
public sealed class ProductGroupsWithAxesSpec : Specification<ProductGroup>
{
    public ProductGroupsWithAxesSpec(IReadOnlyCollection<ProductGroupId> modelIds)
        => Query.Where(group => modelIds.Contains(group.Id)).Include(group => group.Axes);
}
