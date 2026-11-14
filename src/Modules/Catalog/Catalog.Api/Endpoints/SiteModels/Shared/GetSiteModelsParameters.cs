using Catalog.Contracts.SiteModels;
namespace Catalog.Api.Endpoints.SiteModels.Shared;

/// <summary>
/// The query of the admin «Моделі» list and showcase; a missing value takes the default of the list request.
/// </summary>
public sealed record GetSiteModelsParameters
{
    public int? CategoryId { get; init; }
    public string? Status { get; init; }
    public string? Problem { get; init; }
    public string? Search { get; init; }
    public int? Page { get; init; }
    public int? PageSize { get; init; }

    public GetSiteModelsRequest ToModelsRequest()
    {
        var defaults = new GetSiteModelsRequest();
        return defaults with
        {
            CategoryId = CategoryId, Status = Status, Problem = Problem, Search = Search,
            Page = Page ?? defaults.Page, PageSize = PageSize ?? defaults.PageSize
        };
    }

    public GetSiteShowcaseRequest ToShowcaseRequest()
    {
        var defaults = new GetSiteShowcaseRequest();
        return defaults with
        {
            CategoryId = CategoryId, Status = Status, Problem = Problem, Search = Search,
            Page = Page ?? defaults.Page, PageSize = PageSize ?? defaults.PageSize
        };
    }
}
