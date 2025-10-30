namespace Application.Catalog.GetProducts;

public sealed class PageRequestValidator : AbstractValidator<ProductPagination>
{
    public PageRequestValidator()
    {
        RuleFor(request => request.CurrentPage)
            .GreaterThan(0).WithMessage("Номер сторінки має бути більше 0.");

        RuleFor(request => request)
            .Must(request => request.CurrentPage < request.PageSize)
            .WithMessage("Номер сторінки не може бути більшим за розмір сторінки.");

        RuleFor(request => request.PageSize)
            .InclusiveBetween(1, 200).WithMessage("Розмір сторінки має бути від 1 до 200.");
    }
}
