namespace Catalog.Application.Features.SiteModels.Queries.GetSiteModel;

public sealed class GetSiteModelQueryValidator : AbstractValidator<GetSiteModelQuery>
{
    public GetSiteModelQueryValidator() => RuleFor(query => query.ModelId).GreaterThan(0);
}
