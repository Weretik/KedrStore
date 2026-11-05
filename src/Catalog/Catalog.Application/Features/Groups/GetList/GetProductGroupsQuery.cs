using Catalog.Contracts.Groups;

namespace Catalog.Application.Features.Groups.GetList;

/// <summary>Up to <see cref="MaxResults"/> groups by category and part of the name, for the group picker.</summary>
public sealed record GetProductGroupsQuery(GetAdminProductGroupsRequest Request)
    : IQuery<Result<List<AdminProductGroupListItemDto>>>
{
    public const int MaxResults = 50;
}
