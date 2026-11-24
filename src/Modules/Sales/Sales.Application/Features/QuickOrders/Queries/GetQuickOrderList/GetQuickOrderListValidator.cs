namespace Sales.Application.Features.QuickOrders.Queries.GetQuickOrderList;

public sealed class GetQuickOrderListValidator : AbstractValidator<GetQuickOrderListQuery>
{
    public GetQuickOrderListValidator()
    {
        RuleFor(query => query.Request).NotNull();
        When(query => query.Request is not null, () =>
        {
            RuleFor(query => query.Request.Page)
                .GreaterThanOrEqualTo(1).WithMessage("Номер сторінки має бути не менше 1.");
            RuleFor(query => query.Request.PageSize)
                .InclusiveBetween(1, 100).WithMessage("Розмір сторінки — від 1 до 100.");
            RuleFor(query => query.Request.Search)
                .MaximumLength(100).WithMessage("Пошуковий запит — не більше 100 символів.");
            RuleFor(query => query.Request.Delivery)
                .Must(delivery => delivery is null or QuickOrderListRequest.ProblemDelivery)
                .WithMessage("Невідомий фільтр відправки.");
        });
    }
}
