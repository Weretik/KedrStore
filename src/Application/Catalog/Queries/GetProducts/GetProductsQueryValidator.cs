namespace Application.Catalog.Queries.GetProducts;

public sealed class GetProductsQueryValidator
    : AbstractValidator<GetProductsQuery>
{
    public GetProductsQueryValidator()
    {
        RuleFor(q => q.PageNumber)
            .GreaterThan(0).WithMessage("Номер сторінки має бути більше 0.");

        RuleFor(q => q.PageSize)
            .InclusiveBetween(1, 100).WithMessage("Розмір сторінки має бути від 1 до 100.");

        RuleFor(q => q.MinPrice)
            .GreaterThanOrEqualTo(0).When(q => q.MinPrice.HasValue)
            .WithMessage("Мінімальна ціна не може бути менше 0.");

        RuleFor(q => q.MaxPrice)
            .GreaterThanOrEqualTo(0).When(q => q.MaxPrice.HasValue)
            .WithMessage("Максимальна ціна не може бути менше нуля");

        RuleFor(q => q)
            .Must(q => !q.MinPrice.HasValue || !q.MaxPrice.HasValue || q.MinPrice <= q.MaxPrice)
            .WithMessage("Мінімальна ціна не може бути більшою за максимальну.");

        RuleFor(q => q.Sort)
            .Must(d => d == AppSortDirection.Asc || d == AppSortDirection.Desc)
            .WithMessage("Напрямок сортування має бути 'asc' або 'desc'.");

        RuleFor(q => q.SortBy)
            .MaximumLength(50).When(q => !string.IsNullOrWhiteSpace(q.SortBy))
            .WithMessage("Поле сортування не може перевищувати 50 символів.");

        RuleFor(q => q.SearchTerm)
            .MaximumLength(100).When(q => !string.IsNullOrWhiteSpace(q.SearchTerm))
            .WithMessage("Пошуковий запит не може перевищувати 100 символів.");


        //TODO: Добавить првоерку сортировки по полям
        /*
        RuleFor(q => q.SortBy)
            .Must(field => new[] { "name", "price", "created" }.Contains(field))
            .When(q => !string.IsNullOrWhiteSpace(q.SortBy))
            .WithMessage("Поле сортування недійсне.");
        */
    }
}
