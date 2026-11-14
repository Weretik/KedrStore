using Catalog.Application.Shared;

namespace Catalog.Application.Features.SiteModels.Queries.GetPublicSiteModel;

public sealed class GetPublicSiteModelValidator : AbstractValidator<GetPublicSiteModelQuery>
{
    public GetPublicSiteModelValidator()
    {
        RuleFor(x => x.Lang).SupportedLanguage();
        RuleFor(x => x.Slug).LookupSlug();
    }
}
