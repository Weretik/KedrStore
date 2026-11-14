using Sales.Application.Features.OrderWorkspace.Shared;

namespace Sales.Application.Features.OrderWorkspace.Queries.GetWorkspaceCustomerPrices;

public sealed class GetWorkspaceCustomerPricesValidator : AbstractValidator<GetWorkspaceCustomerPricesQuery>
{
    private const string Message = "Оберіть клієнта і від 1 до 100 товарів.";

    public GetWorkspaceCustomerPricesValidator()
    {
        RuleFor(query => query.Request).NotNull().WithErrorCode("invalid_products").WithMessage(Message);
        When(query => query.Request is not null, () =>
        {
            RuleFor(query => query.Request.CounterpartyId).Must(WorkspaceInputRules.IsCounterpartyId)
                .WithErrorCode("invalid_products").WithMessage(Message);
            RuleFor(query => query.Request.ProductIds)
                .Must(ids => ids is { Count: >= 1 and <= WorkspaceInputRules.MaxLines } &&
                             ids.All(WorkspaceInputRules.IsProductId))
                .WithErrorCode("invalid_products").WithMessage(Message);
        });
    }
}
