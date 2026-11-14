namespace Sales.Application.Features.Orders.AdminWorkspace.Orders.Quote.Validators;

public sealed class QuoteWorkspaceOrderQueryValidator : AbstractValidator<QuoteWorkspaceOrderQuery>
{
    public QuoteWorkspaceOrderQueryValidator()
    {
        RuleFor(query => query.Quote).NotNull().WithErrorCode("invalid_lines")
            .WithMessage(WorkspaceInputRules.LinesMessage);
        When(query => query.Quote is not null, () =>
            this.AddLineRules(query => query.Quote.CounterpartyId, query => query.Quote.Lines));
    }
}
