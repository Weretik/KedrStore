namespace Application.Catalog.Queries.GetProducts;

public sealed class ProductsFilterValidator : AbstractValidator<ProductsFilter>
{
    public ProductsFilterValidator()
    {
        RuleLevelCascadeMode = CascadeMode.Stop;

        RuleFor(filter => filter.SearchTerm)
            .MaximumLength(100).When(filter => !string.IsNullOrWhiteSpace(filter.SearchTerm))
            .WithMessage("Пошуковий запит не може перевищувати 100 символів.");

        RuleFor(filter => filter.MinPrice)
            .GreaterThanOrEqualTo(0).When(filter => filter.MinPrice.HasValue)
            .WithMessage("Мінімальна ціна не може бути менше 0.");

        RuleFor(filter => filter.MaxPrice)
            .GreaterThanOrEqualTo(0).When(filter => filter.MaxPrice.HasValue)
            .WithMessage("Максимальна ціна не може бути менше нуля");

        RuleFor(filter => filter)
            .Must(filter => !filter.MinPrice.HasValue
                            || !filter.MaxPrice.HasValue
                            || filter.MinPrice
                            <= filter.MaxPrice)
            .WithMessage("Мінімальна ціна не може бути більшою за максимальну.");
    }
}
