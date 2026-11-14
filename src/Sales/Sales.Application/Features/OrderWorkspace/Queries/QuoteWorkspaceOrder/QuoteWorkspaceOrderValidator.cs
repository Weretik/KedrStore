using Sales.Application.Features.OrderWorkspace.Shared;

namespace Sales.Application.Features.OrderWorkspace.Queries.QuoteWorkspaceOrder;

public sealed class QuoteWorkspaceOrderValidator : AbstractValidator<QuoteWorkspaceOrderQuery>
{
    public QuoteWorkspaceOrderValidator()
    {
        RuleFor(query => query.Quote).NotNull().WithErrorCode("invalid_lines")
            .WithMessage(WorkspaceInputRules.LinesMessage);
        When(query => query.Quote is not null, () =>
            this.AddLineRules(query => query.Quote.CounterpartyId, query => query.Quote.Lines));
    }
}
