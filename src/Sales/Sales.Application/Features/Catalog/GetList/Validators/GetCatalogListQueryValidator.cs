namespace Sales.Application.Features.Catalog.GetList.Validators;

public sealed class GetCatalogListQueryValidator : AbstractValidator<GetCatalogListQuery>
{
    public GetCatalogListQueryValidator()
    {
        RuleFor(x => x.Request.IdentityUserId).NotEqual(Guid.Empty).When(x => x.Request.IdentityUserId.HasValue);
        RuleFor(x => x.Request.Page).GreaterThan(0);
        RuleFor(x => x.Request.PageSize).GreaterThan(0).LessThanOrEqualTo(100);
    }
}
