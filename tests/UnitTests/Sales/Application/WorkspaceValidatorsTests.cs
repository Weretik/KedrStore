using Sales.Application.Contracts.Orders;
using Sales.Application.Features.Orders.AdminWorkspace.Drafts.Create;
using Sales.Application.Features.Orders.AdminWorkspace.Drafts.Create.Validators;
using Sales.Application.Features.Orders.AdminWorkspace.Orders.CustomerPrices;
using Sales.Application.Features.Orders.AdminWorkspace.Orders.CustomerPrices.Validators;
using Sales.Application.Features.Orders.AdminWorkspace.Orders.GetList;
using Sales.Application.Features.Orders.AdminWorkspace.Orders.GetList.Validators;
using Sales.Application.Features.Orders.AdminWorkspace.Orders.Quote;
using Sales.Application.Features.Orders.AdminWorkspace.Orders.Quote.Validators;

namespace UnitTests.Sales.Application;

/// <summary>Each workspace failure carries the public code the admin uses to word it.</summary>
public sealed class WorkspaceValidatorsTests
{
    private static string? FirstCode<T>(FluentValidation.IValidator<T> validator, T message)
        => validator.Validate(message).Errors.Select(error => error.ErrorCode).FirstOrDefault();

    private static QuoteWorkspaceOrderQuery Quote(params WorkspaceInputLine[] lines)
        => new(new WorkspaceQuoteRequest("customer", lines));

    [Fact]
    public void Quote_AcceptsWholeQuantitiesAndKopeckAdjustments()
        => Assert.Null(FirstCode(new QuoteWorkspaceOrderQueryValidator(),
            Quote(new WorkspaceInputLine("101", 2, -10.5m), new WorkspaceInputLine("102", 1))));

    [Theory]
    [InlineData("0", 1)]
    [InlineData("0101", 1)]
    [InlineData("-5", 1)]
    [InlineData("101", 0)]
    public void Quote_RejectsBadLines(string productId, int quantity)
        => Assert.Equal("invalid_lines", FirstCode(new QuoteWorkspaceOrderQueryValidator(),
            Quote(new WorkspaceInputLine(productId, quantity))));

    [Fact]
    public void Quote_RejectsTheSameProductTwice()
        => Assert.Equal("invalid_lines", FirstCode(new QuoteWorkspaceOrderQueryValidator(),
            Quote(new WorkspaceInputLine("101", 1), new WorkspaceInputLine("101", 2))));

    [Fact]
    public void Quote_RejectsAnEmptyCustomer()
        => Assert.Equal("invalid_lines", FirstCode(new QuoteWorkspaceOrderQueryValidator(),
            new QuoteWorkspaceOrderQuery(new WorkspaceQuoteRequest(" ", [new WorkspaceInputLine("101", 1)]))));

    [Theory]
    [InlineData(0.001)]
    [InlineData(1_000_000.01)]
    [InlineData(-1_000_000.01)]
    public void Quote_RejectsAdjustmentsOutsideWholeKopecksOrTheLimit(double adjustment)
        => Assert.Equal("invalid_price_adjustment", FirstCode(new QuoteWorkspaceOrderQueryValidator(),
            Quote(new WorkspaceInputLine("101", 1, (decimal)adjustment))));

    [Theory]
    [InlineData(" ", "101")]
    [InlineData("customer", "abc")]
    public void CustomerPrices_RejectAMissingCustomerOrBadProduct(string customer, string productId)
        => Assert.Equal("invalid_products", FirstCode(new GetWorkspaceCustomerPricesQueryValidator(),
            new GetWorkspaceCustomerPricesQuery(new WorkspaceCustomerPricesRequest(customer, [productId]))));

    [Fact]
    public void CreateDraft_NeedsAnIdempotencyKeyAndTitle()
    {
        var draft = new WorkspaceDraftInput(" ", null, "customer", [new WorkspaceInputLine("101", 1)]);
        var validator = new CreateWorkspaceDraftCommandValidator();

        Assert.Equal("invalid_idempotency_key", FirstCode(validator, new CreateWorkspaceDraftCommand(draft, Guid.Empty)));
        Assert.Equal("invalid_draft", FirstCode(validator, new CreateWorkspaceDraftCommand(draft, Guid.NewGuid())));
    }

    [Theory]
    [InlineData("2026-03-10", "2026-03-09", "-createdAt", 1, "invalid_date_range")]
    [InlineData(null, null, "price", 1, "invalid_sort")]
    [InlineData(null, null, "-createdAt", 0, "invalid_query")]
    [InlineData("2026-03-09", "2026-03-10", "totalAmount", 1, null)]
    public void OrderList_ChecksDatesSortAndPage(string? from, string? to, string sort, int page, string? code)
        => Assert.Equal(code, FirstCode(new GetWorkspaceOrdersQueryValidator(),
            new GetWorkspaceOrdersQuery(null, null, from, to, sort, page, 20)));
}
