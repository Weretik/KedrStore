namespace Application.Catalog.Queries.GetProducts;

public sealed class GetProductsQueryValidator
    : AbstractValidator<GetProductsQuery>
{
    public GetProductsQueryValidator()
    {
        RuleFor(q => q.PageNumber)
            .GreaterThan(0).WithMessage("Номер сторінки має бути більше 0.");

        RuleFor(q => q.PageSize)
            .InclusiveBetween(1, 200).WithMessage("Розмір сторінки має бути від 1 до 200.");

        RuleFor(q => q.MinPrice)
            .GreaterThanOrEqualTo(0).When(q => q.MinPrice.HasValue)
            .WithMessage("Мінімальна ціна не може бути менше 0.");

        RuleFor(q => q.MaxPrice)
            .GreaterThanOrEqualTo(0).When(q => q.MaxPrice.HasValue)
            .WithMessage("Максимальна ціна не може бути менше нуля");

        RuleFor(q => q)
            .Must(q => !q.MinPrice.HasValue || !q.MaxPrice.HasValue || q.MinPrice <= q.MaxPrice)
            .WithMessage("Мінімальна ціна не може бути більшою за максимальну.");

        RuleFor(q => q.SearchTerm)
            .MaximumLength(100).When(q => !string.IsNullOrWhiteSpace(q.SearchTerm))
            .WithMessage("Пошуковий запит не може перевищувати 100 символів.");

        RuleFor(q => q.Sort)
            .Must(BeValidSort)
            .WithMessage("Некоректний параметр сортування.");

    }
    private static bool BeValidSort(string? sort)
    {
        if (string.IsNullOrWhiteSpace(sort))
            return true;

        var map = new ProductSortMap();
        var parsed = SortParser.Parse(sort, map.DefaultKey);

        return parsed.Any();
    }
}
