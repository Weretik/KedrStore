namespace Catalog.Application.Features.SiteModels.Queries.GetPublicSiteModels;

public sealed class GetPublicSiteModelsQueryValidator : AbstractValidator<GetPublicSiteModelsQuery>
{
    public GetPublicSiteModelsQueryValidator()
    {
        RuleFor(query => query.Request).NotNull();
        When(query => query.Request is not null, () =>
        {
            RuleFor(query => query.Request.Page).GreaterThan(0).OverridePropertyName("page");
            RuleFor(query => query.Request.PageSize).InclusiveBetween(1, GetPublicSiteModelsQuery.MaxPageSize)
                .OverridePropertyName("pageSize");
            RuleFor(query => query.Request.Search).MaximumLength(200).OverridePropertyName("search");
            RuleFor(query => query.Request.Sort)
                .Must(sort => sort is null || GetPublicSiteModelsQuery.Sorts.Contains(sort))
                .WithMessage("Sort is default, price_asc, price_desc, name_asc, or new.").OverridePropertyName("sort");
            RuleFor(query => query.Request.PriceFrom).GreaterThanOrEqualTo(0).OverridePropertyName("priceFrom");
            RuleFor(query => query.Request.PriceTo).GreaterThanOrEqualTo(0).OverridePropertyName("priceTo");
            RuleFor(query => query.Request.Filter)
                .Must(filter => PublicModelFilter.Parse(filter) is not null)
                .WithMessage($"Use attribute-slug:value-slug[,value-slug], at most {PublicModelFilter.MaxValues} values.")
                .OverridePropertyName("filter");
        });
    }
}
