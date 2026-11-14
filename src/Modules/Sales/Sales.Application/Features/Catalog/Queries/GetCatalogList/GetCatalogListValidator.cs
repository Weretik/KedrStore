namespace Sales.Application.Features.Catalog.Queries.GetCatalogList;

public sealed class GetCatalogListValidator : AbstractValidator<GetCatalogListQuery>
{
    public GetCatalogListValidator()
    {
        RuleFor(x => x.Request.IdentityUserId).NotEqual(Guid.Empty).When(x => x.Request.IdentityUserId.HasValue);
        RuleFor(x => x.Request.Page).GreaterThan(0);
        RuleFor(x => x.Request.PageSize).GreaterThan(0).LessThanOrEqualTo(100);
    }
}
