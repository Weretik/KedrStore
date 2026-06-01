using Catalog.Application.Features.Products.GetSalesList;

namespace Catalog.Application.Features.Products.GetSalesList.Validators;

public sealed class GetProductsQueryValidator : AbstractValidator<GetProductListQuery>
{
    public GetProductsQueryValidator()
    {
        const int maxSearchTermLength = 100;
        const int maxPageSize = 100;
        RuleLevelCascadeMode = CascadeMode.Stop;

        RuleFor(x => x.Request)
            .NotNull()
            .WithMessage("Request cannot be empty.");

        When(x => true, () =>
        {
            RuleFor(x => x.Request.SearchTerm)
                .MaximumLength(maxSearchTermLength)
                .WithMessage($"Search term cannot exceed {maxSearchTermLength} characters.");

            RuleFor(x => x.Request.Sort)
                .IsInEnum()
                .WithMessage("Invalid sort parameter.");

            RuleFor(x => x.Request.Page)
                .GreaterThanOrEqualTo(1)
                .WithMessage("Page number must be >= 1.");

            RuleFor(x => x.Request.PageSize)
                .GreaterThanOrEqualTo(1)
                .WithMessage("Page size must be >= 1.")
                .LessThanOrEqualTo(maxPageSize)
                .WithMessage($"Page size cannot exceed {maxPageSize}.");

            RuleFor(x => x.Request.CategorySlug)
                .MaximumLength(maxSearchTermLength)
                .WithMessage($"Category cannot exceed {maxSearchTermLength} characters.");

            RuleFor(x => x.Request.CategoryId)
                .GreaterThan(0)
                .When(x => x.Request.CategoryId.HasValue)
                .WithMessage("Invalid category.");

            RuleFor(x => x.Request.PriceTypeId)
                .GreaterThan(0)
                .When(x => x.Request.PriceTypeId.HasValue)
                .WithMessage("Invalid price type.");

            RuleForEach(x => x.Request.PriceTypeRules).ChildRules(rule =>
            {
                rule.RuleFor(x => x.CategoryId)
                    .GreaterThan(0)
                    .WithMessage("Invalid category for price type rule.");

                rule.RuleFor(x => x.PriceTypeId)
                    .GreaterThan(0)
                    .WithMessage("Invalid price type for category rule.");
            });

            RuleFor(x => x.Request.PriceFrom)
                .GreaterThanOrEqualTo(0)
                .When(x => x.Request.PriceFrom.HasValue)
                .WithMessage("Minimum price cannot be less than 0.");

            RuleFor(x => x.Request.PriceTo)
                .GreaterThanOrEqualTo(0)
                .When(x => x.Request.PriceTo.HasValue)
                .WithMessage("Maximum price cannot be less than 0.");

            RuleFor(x => x.Request.Lang)
                .Must(lang => string.IsNullOrWhiteSpace(lang) ||
                              lang.Equals("uk", StringComparison.OrdinalIgnoreCase) ||
                              lang.Equals("ru", StringComparison.OrdinalIgnoreCase))
                .WithMessage("Lang parameter can only be 'uk' or 'ru'.");

            RuleFor(x => x.Request)
                .Must(r => !r.PriceFrom.HasValue || !r.PriceTo.HasValue || r.PriceFrom <= r.PriceTo)
                .WithMessage("Minimum price cannot be greater than maximum price.");
        });
    }
}
