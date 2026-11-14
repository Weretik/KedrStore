namespace Catalog.Application.Features.SiteModels.Queries.GetSiteModel;

public sealed class GetSiteModelValidator : AbstractValidator<GetSiteModelQuery>
{
    public GetSiteModelValidator() => RuleFor(query => query.ModelId).GreaterThan(0);
}
