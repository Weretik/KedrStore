using Catalog.Contracts.Products.GetList;

namespace Catalog.Application.Features.Products.Shared;

/// <summary>Input rules of a product list request, shared by the sales and the public product lists.</summary>
public sealed class GetProductsRequestValidator : AbstractValidator<GetProductsRequest>
{
    public GetProductsRequestValidator()
    {
        const int maxSearchTermLength = 100;
        const int maxPageSize = 100;
        RuleLevelCascadeMode = CascadeMode.Stop;

        RuleFor(x => x.SearchTerm)
            .MaximumLength(maxSearchTermLength)
            .WithMessage($"Пошуковий запит — не більше {maxSearchTermLength} символів.");

        RuleFor(x => x.Sort)
            .IsInEnum()
            .WithMessage("Некоректне сортування.");

        RuleFor(x => x.Page)
            .GreaterThanOrEqualTo(1)
            .WithMessage("Номер сторінки має бути не менше 1.");

        RuleFor(x => x.PageSize)
            .GreaterThanOrEqualTo(1)
            .WithMessage("Розмір сторінки має бути не менше 1.")
            .LessThanOrEqualTo(maxPageSize)
            .WithMessage($"Розмір сторінки — не більше {maxPageSize}.");

        RuleFor(x => x.CategorySlug)
            .MaximumLength(maxSearchTermLength)
            .WithMessage($"Категорія — не більше {maxSearchTermLength} символів.");

        RuleFor(x => x.CategoryId)
            .GreaterThan(0)
            .When(x => x.CategoryId.HasValue)
            .WithMessage("Некоректна категорія.");

        RuleFor(x => x.PriceTypeId)
            .GreaterThan(0)
            .When(x => x.PriceTypeId.HasValue)
            .WithMessage("Некоректний тип ціни.");

        RuleForEach(x => x.PriceTypeRules).ChildRules(rule =>
        {
            rule.RuleFor(x => x.CategoryId)
                .GreaterThan(0)
                .WithMessage("Некоректна категорія для правила типу ціни.");

            rule.RuleFor(x => x.PriceTypeId)
                .GreaterThan(0)
                .WithMessage("Некоректний тип ціни для правила категорії.");
        });

        RuleFor(x => x.PriceFrom)
            .GreaterThanOrEqualTo(0)
            .When(x => x.PriceFrom.HasValue)
            .WithMessage("Мінімальна ціна не може бути менше 0.");

        RuleFor(x => x.PriceTo)
            .GreaterThanOrEqualTo(0)
            .When(x => x.PriceTo.HasValue)
            .WithMessage("Максимальна ціна не може бути менше 0.");

        RuleFor(x => x.Lang)
            .Must(lang => string.IsNullOrWhiteSpace(lang) ||
                          lang.Equals("uk", StringComparison.OrdinalIgnoreCase) ||
                          lang.Equals("ru", StringComparison.OrdinalIgnoreCase))
            .WithMessage("Мова може бути лише uk або ru.");

        RuleFor(x => x)
            .Must(r => !r.PriceFrom.HasValue || !r.PriceTo.HasValue || r.PriceFrom <= r.PriceTo)
            .WithMessage("Мінімальна ціна не може бути більшою за максимальну.");
    
    }
}
