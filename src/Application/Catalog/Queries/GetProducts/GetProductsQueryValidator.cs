namespace Application.Catalog.Queries.GetProducts;

public sealed class GetProductsQueryValidator : AbstractValidator<GetProductsQuery>
{
    public GetProductsQueryValidator(
        IValidator<ProductsFilter> filterValidator,
        IValidator<PageRequest> pageValidator,
        ISortMap<Product> sortMap)
    {
        RuleLevelCascadeMode = CascadeMode.Stop;

        RuleFor(query => query.Filter).SetValidator(filterValidator);
        RuleFor(query => query.PageRequest).SetValidator(pageValidator);
        RuleFor(query => query.Sort)
            .Must(s => s.IsValidSort(sortMap)).WithMessage("Некоректний параметр сортування.");
    }
}
