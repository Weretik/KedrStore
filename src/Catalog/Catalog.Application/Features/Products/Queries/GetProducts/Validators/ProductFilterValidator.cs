using Catalog.Application.Features.Products.Queries.GetProducts;

namespace Catalog.Application.GetProducts;

public sealed class ProductFilterValidator : AbstractValidator<ProductFilter>
{
    public ProductFilterValidator()
    {
        RuleLevelCascadeMode = CascadeMode.Stop;

        RuleFor(filter => filter.SearchTerm)
            .MaximumLength(100).When(filter => !string.IsNullOrWhiteSpace(filter.SearchTerm))
            .WithMessage("Пошуковий запит не може перевищувати 100 символів.");
    }
}
