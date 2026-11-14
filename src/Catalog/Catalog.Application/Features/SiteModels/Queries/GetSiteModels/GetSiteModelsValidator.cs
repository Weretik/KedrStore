using Catalog.Application.Features.SiteModels.Shared;

namespace Catalog.Application.Features.SiteModels.Queries.GetSiteModels;

public sealed class GetSiteModelsValidator : AbstractValidator<GetSiteModelsQuery>
{
    public GetSiteModelsValidator()
    {
        RuleFor(query => query.Request).NotNull();
        When(query => query.Request is not null, () =>
        {
            RuleFor(query => query.Request.Page).GreaterThan(0).OverridePropertyName("page");
            RuleFor(query => query.Request.PageSize).InclusiveBetween(1, GetSiteModelsQuery.MaxPageSize)
                .OverridePropertyName("pageSize");
            RuleFor(query => query.Request.Status).Must(status => status is null or "proposed" or "accepted")
                .WithMessage("Статус: proposed або accepted.").OverridePropertyName("status");
            RuleFor(query => query.Request.Problem).Must(problem => problem is null || problem == SiteModelProblems.Any || SiteModelProblems.All.Contains(problem))
                .WithMessage("Невідомий код проблеми.").OverridePropertyName("problem");
            RuleFor(query => query.Request.Search).MaximumLength(200).OverridePropertyName("search");
        });
    }
}
