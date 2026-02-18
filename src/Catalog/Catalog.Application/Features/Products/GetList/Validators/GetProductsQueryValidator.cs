namespace Catalog.Application.Features.Products.GetList.Validators;

public sealed class GetProductsQueryValidator : AbstractValidator<GetProductListQuery>
{
    public GetProductsQueryValidator()
    {
        const int maxSearchTermLength = 100;
        const int maxPageSize = 100;
        RuleLevelCascadeMode = CascadeMode.Stop;

        RuleFor(x => x.Request)
            .NotNull()
            .WithMessage("Request не може бути порожнім.");

        When(x => true, () =>
        {
            RuleFor(x => x.Request.SearchTerm)
                .MaximumLength(maxSearchTermLength)
                .WithMessage($"Пошуковий запит не може перевищувати {maxSearchTermLength} символів.");

            RuleFor(x => x.Request.Sort)
                .IsInEnum()
                .WithMessage("Некоректний параметр сортування.");

            RuleFor(x => x.Request.Page)
                .GreaterThanOrEqualTo(1)
                .WithMessage("Номер сторінки має бути >= 1.");

            RuleFor(x => x.Request.PageSize)
                .GreaterThanOrEqualTo(1)
                .WithMessage("Розмір сторінки має бути >= 1.")
                .LessThanOrEqualTo(maxPageSize)
                .WithMessage($"Розмір сторінки не може бути більше {maxPageSize}.");

            RuleFor(x => x.Request.CategorySlug)
                .MaximumLength(maxSearchTermLength)
                .WithMessage($"Категорія не може перевищувати {maxSearchTermLength} символів.");

            RuleFor(x => x.Request.PriceTypeId)
                .GreaterThan(0)
                .WithMessage("Некоректний тип ціни (PriceTypeId).");

            RuleFor(x => x.Request.PriceFrom)
                .GreaterThanOrEqualTo(0)
                .When(x => x.Request.PriceFrom.HasValue)
                .WithMessage("Мінімальна ціна не може бути менше 0.");

            RuleFor(x => x.Request.PriceTo)
                .GreaterThanOrEqualTo(0)
                .When(x => x.Request.PriceTo.HasValue)
                .WithMessage("Максимальна ціна не може бути менше 0.");

            RuleFor(x => x.Request)
                .Must(r => !r.PriceFrom.HasValue || !r.PriceTo.HasValue || r.PriceFrom <= r.PriceTo)
                .WithMessage("Мінімальна ціна не може бути більшою за максимальну.");
        });
    }
}
