namespace Catalog.Application.Features.SiteModels.Get;

public sealed class GetSiteModelQueryValidator : AbstractValidator<GetSiteModelQuery>
{
    public GetSiteModelQueryValidator() => RuleFor(query => query.ModelId).GreaterThan(0);
}
