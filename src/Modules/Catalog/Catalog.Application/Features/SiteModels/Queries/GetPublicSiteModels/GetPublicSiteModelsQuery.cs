using Catalog.Contracts.SiteModels.Public;

namespace Catalog.Application.Features.SiteModels.Queries.GetPublicSiteModels;

/// <summary>Model cards of a category, or the search over all models when <c>CategorySlug</c> is empty.</summary>
public sealed record GetPublicSiteModelsQuery(string Lang, string? CategorySlug, GetPublicSiteModelsRequest Request)
    : IQuery<Result<PublicSiteModelListDto>>
{
    public const int MaxPageSize = 100;
    public static readonly IReadOnlyList<string> Sorts = ["default", "price_asc", "price_desc", "name_asc", "new"];
}
