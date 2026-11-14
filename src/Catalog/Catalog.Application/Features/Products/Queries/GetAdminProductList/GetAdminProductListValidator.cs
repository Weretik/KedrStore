namespace Catalog.Application.Features.Products.Queries.GetAdminProductList;

public sealed class GetAdminProductListValidator : AbstractValidator<GetAdminProductListQuery>
{
    public GetAdminProductListValidator()
    {
        const int maxSearchTermLength = 100;
        const int maxPageSize = 100;

        RuleLevelCascadeMode = CascadeMode.Stop;

        RuleFor(query => query.Request)
            .NotNull()
            .WithMessage("Запит не може бути порожнім.");

        When(query => query.Request is not null, () =>
        {
            RuleFor(query => query.Request.SearchTerm)
                .MaximumLength(maxSearchTermLength)
                .WithMessage($"Пошуковий запит — не більше {maxSearchTermLength} символів.");

            RuleFor(query => query.Request.Sort)
                .IsInEnum()
                .WithMessage("Некоректне сортування.");

            RuleFor(query => query.Request.Page)
                .GreaterThanOrEqualTo(1)
                .WithMessage("Номер сторінки має бути не менше 1.");

            RuleFor(query => query.Request.PageSize)
                .GreaterThanOrEqualTo(1)
                .WithMessage("Розмір сторінки має бути не менше 1.")
                .LessThanOrEqualTo(maxPageSize)
                .WithMessage($"Розмір сторінки — не більше {maxPageSize}.");

            RuleFor(query => query.Request.ProblemReason)
                .MaximumLength(64)
                .WithMessage("Причина проблеми — не більше 64 символів.");

            RuleFor(query => query.Request.CategorySlug)
                .MaximumLength(maxSearchTermLength)
                .WithMessage($"Категорія — не більше {maxSearchTermLength} символів.");

            RuleFor(query => query.Request.CategoryId)
                .GreaterThan(0)
                .When(query => query.Request.CategoryId.HasValue)
                .WithMessage("Некоректна категорія.");

            RuleFor(query => query.Request.PriceFrom)
                .GreaterThanOrEqualTo(0)
                .When(query => query.Request.PriceFrom.HasValue)
                .WithMessage("Мінімальна ціна не може бути менше 0.");

            RuleFor(query => query.Request.PriceTo)
                .GreaterThanOrEqualTo(0)
                .When(query => query.Request.PriceTo.HasValue)
                .WithMessage("Максимальна ціна не може бути менше 0.");

            RuleFor(query => query.Request)
                .Must(request => !request.PriceFrom.HasValue ||
                                 !request.PriceTo.HasValue ||
                                 request.PriceFrom <= request.PriceTo)
                .WithMessage("Мінімальна ціна не може бути більшою за максимальну.");
        });
    }
}
