using Catalog.Application.GetProducts;

namespace Catalog.Application.Features.Products.Queries.GetProducts;

public sealed class GetProductsQueryValidator : AbstractValidator<GetProductsQuery>
{
    public GetProductsQueryValidator(IValidator<ProductFilter> filterValidator, IValidator<ProductPagination> pageValidator)
    {
        RuleLevelCascadeMode = CascadeMode.Stop;

        RuleFor(query => query.Filter)
            .SetValidator(filterValidator);

        RuleFor(query => query.Pagination)
            .SetValidator(pageValidator);

        RuleFor(query => query.Sorter.Key)
            .IsInEnum()
            .WithMessage("Некоректний параметр сортування.");

        //RuleFor(query => query.PricingOptions.PriceType).InclusiveBetween(1, 12).WithMessage("Для сортування за ціною має бути заданий коректний PriceTypeId (1-12).");

        RuleFor(filter => filter.PricingOptions.MinPrice)
            .GreaterThanOrEqualTo(0).When(filter => filter.PricingOptions.MinPrice.HasValue)
            .WithMessage("Мінімальна ціна не може бути менше 0.");

        RuleFor(filter => filter.PricingOptions.MaxPrice)
            .GreaterThanOrEqualTo(0).When(filter => filter.PricingOptions.MaxPrice.HasValue)
            .WithMessage("Максимальна ціна не може бути менше нуля");

        RuleFor(filter => filter)
            .Must(filter => !filter.PricingOptions.MinPrice.HasValue
                            || !filter.PricingOptions.MaxPrice.HasValue
                            || filter.PricingOptions.MinPrice
                            <= filter.PricingOptions.MaxPrice)
            .WithMessage("Мінімальна ціна не може бути більшою за максимальну.");
    }
}
