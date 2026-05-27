namespace Sales.Application.Features.Catalog.GetList.Validators;

public sealed class GetSalesCatalogListQueryValidator : AbstractValidator<GetSalesCatalogListQuery>
{
    public GetSalesCatalogListQueryValidator()
    {
        RuleFor(x => x.Request.Page).GreaterThan(0);
        RuleFor(x => x.Request.PageSize).GreaterThan(0).LessThanOrEqualTo(100);
    }
}
