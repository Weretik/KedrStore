namespace Application.Catalog.GetProducts;

public sealed class GetProductsQueryValidator : AbstractValidator<GetProductsQuery>
{
    public GetProductsQueryValidator(IValidator<ProductFilter> filterValidator, IValidator<ProductPagination> pageValidator)
    {
        RuleLevelCascadeMode = CascadeMode.Stop;

        RuleFor(query => query.Filter)
            .SetValidator(filterValidator);

        RuleFor(query => query.ProductPagination)
            .SetValidator(pageValidator);

        RuleFor(query => query.Sorter.Key)
            .IsInEnum()
            .WithMessage("Некоректний параметр сортування.");

        RuleFor(query => query.PriceTypeId)
            .InclusiveBetween(1, 12)
            .WithMessage("Для сортування за ціною має бути заданий коректний PriceTypeId (1-12).");
    }
}
