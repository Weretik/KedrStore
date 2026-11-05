using Catalog.Application.Features.SiteModels.Common;

namespace Catalog.Application.Features.SiteModels.GetShowcase;

public sealed class GetSiteShowcaseQueryValidator : AbstractValidator<GetSiteShowcaseQuery>
{
    public GetSiteShowcaseQueryValidator()
    {
        RuleFor(query => query.Request).NotNull();
        When(query => query.Request is not null, () =>
        {
            RuleFor(query => query.Request.Page).GreaterThan(0).OverridePropertyName("page");
            RuleFor(query => query.Request.PageSize).InclusiveBetween(1, GetSiteShowcaseQuery.MaxPageSize)
                .OverridePropertyName("pageSize");
            RuleFor(query => query.Request.Status)
                .Must(status => status is null or "proposed" or "accepted" or GetSiteShowcaseQuery.NoModelStatus)
                .WithMessage("Status is proposed, accepted, or none.").OverridePropertyName("status");
            RuleFor(query => query.Request.Problem)
                .Must(problem => problem is null || problem == SiteModelProblems.Any || SiteModelProblems.Product.Contains(problem))
                .WithMessage("Unknown problem code.").OverridePropertyName("problem");
            RuleFor(query => query.Request.Search).MaximumLength(200).OverridePropertyName("search");
        });
    }
}
