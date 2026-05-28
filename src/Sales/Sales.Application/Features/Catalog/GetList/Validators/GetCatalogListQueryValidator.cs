namespace Sales.Application.Features.Catalog.GetList.Validators;

public sealed class GetCatalogListQueryValidator : AbstractValidator<GetCatalogListQuery>
{
    public GetCatalogListQueryValidator()
    {
        RuleFor(x => x.Request.CounterpartyId).MaximumLength(64);
        RuleFor(x => x.Request.Page).GreaterThan(0);
        RuleFor(x => x.Request.PageSize).GreaterThan(0).LessThanOrEqualTo(100);
    }
}
