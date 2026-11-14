namespace Catalog.Application.Features.Products.Queries.GetProductList;

public sealed class GetProductsValidator : AbstractValidator<GetProductListQuery>
{
    public GetProductsValidator()
    {
        const int maxSearchTermLength = 100;
        const int maxPageSize = 100;
        RuleLevelCascadeMode = CascadeMode.Stop;

        RuleFor(x => x.Request)
            .NotNull()
            .WithMessage("Запит не може бути порожнім.");

        When(x => true, () =>
        {
            RuleFor(x => x.Request.SearchTerm)
                .MaximumLength(maxSearchTermLength)
                .WithMessage($"Пошуковий запит — не більше {maxSearchTermLength} символів.");

            RuleFor(x => x.Request.Sort)
                .IsInEnum()
                .WithMessage("Некоректне сортування.");

            RuleFor(x => x.Request.Page)
                .GreaterThanOrEqualTo(1)
                .WithMessage("Номер сторінки має бути не менше 1.");

            RuleFor(x => x.Request.PageSize)
                .GreaterThanOrEqualTo(1)
                .WithMessage("Розмір сторінки має бути не менше 1.")
                .LessThanOrEqualTo(maxPageSize)
                .WithMessage($"Розмір сторінки — не більше {maxPageSize}.");

            RuleFor(x => x.Request.CategorySlug)
                .MaximumLength(maxSearchTermLength)
                .WithMessage($"Категорія — не більше {maxSearchTermLength} символів.");

            RuleFor(x => x.Request.CategoryId)
                .GreaterThan(0)
                .When(x => x.Request.CategoryId.HasValue)
                .WithMessage("Некоректна категорія.");

            RuleFor(x => x.Request.PriceTypeId)
                .GreaterThan(0)
                .When(x => x.Request.PriceTypeId.HasValue)
                .WithMessage("Некоректний тип ціни.");

            RuleForEach(x => x.Request.PriceTypeRules).ChildRules(rule =>
            {
                rule.RuleFor(x => x.CategoryId)
                    .GreaterThan(0)
                    .WithMessage("Некоректна категорія для правила типу ціни.");

                rule.RuleFor(x => x.PriceTypeId)
                    .GreaterThan(0)
                    .WithMessage("Некоректний тип ціни для правила категорії.");
            });

            RuleFor(x => x.Request.PriceFrom)
                .GreaterThanOrEqualTo(0)
                .When(x => x.Request.PriceFrom.HasValue)
                .WithMessage("Мінімальна ціна не може бути менше 0.");

            RuleFor(x => x.Request.PriceTo)
                .GreaterThanOrEqualTo(0)
                .When(x => x.Request.PriceTo.HasValue)
                .WithMessage("Максимальна ціна не може бути менше 0.");

            RuleFor(x => x.Request.Lang)
                .Must(lang => string.IsNullOrWhiteSpace(lang) ||
                              lang.Equals("uk", StringComparison.OrdinalIgnoreCase) ||
                              lang.Equals("ru", StringComparison.OrdinalIgnoreCase))
                .WithMessage("Мова може бути лише uk або ru.");

            RuleFor(x => x.Request)
                .Must(r => !r.PriceFrom.HasValue || !r.PriceTo.HasValue || r.PriceFrom <= r.PriceTo)
                .WithMessage("Мінімальна ціна не може бути більшою за максимальну.");
        });
    }
}
