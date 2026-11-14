using Sales.Application.Abstractions.Orders;
using Sales.Application.Features.OrderWorkspace.Commands.CreateWorkspaceDraft;
using Sales.Application.Features.OrderWorkspace.Commands.DeleteWorkspaceDraft;
using Sales.Application.Features.OrderWorkspace.Commands.UpdateWorkspaceDraft;
using Sales.Application.Features.OrderWorkspace.Queries.GetWorkspaceCustomerPrices;
using Sales.Application.Features.OrderWorkspace.Queries.GetWorkspaceOrders;
using Sales.Application.Features.OrderWorkspace.Queries.QuoteWorkspaceOrder;
using Sales.Application.Abstractions.Workspace;
using Sales.Application.Abstractions.Workspace.Pricing;
using Sales.Application.Abstractions.Workspace.Drafts;
namespace UnitTests.Sales.Application.Workspace;

/// <summary>Each workspace failure carries the public code the admin uses to word it.</summary>
public sealed class WorkspaceValidatorsTests
{
    private static string? FirstCode<T>(FluentValidation.IValidator<T> validator, T message)
        => validator.Validate(message).Errors.Select(error => error.ErrorCode).FirstOrDefault();

    private static QuoteWorkspaceOrderQuery Quote(params WorkspaceInputLine[] lines)
        => new(new WorkspaceQuoteRequest("customer", lines));

    [Fact]
    public void Quote_AcceptsWholeQuantitiesAndKopeckAdjustments()
        => Assert.Null(FirstCode(new QuoteWorkspaceOrderValidator(),
            Quote(new WorkspaceInputLine("101", 2, -10.5m), new WorkspaceInputLine("102", 1))));

    [Theory]
    [InlineData("0", 1)]
    [InlineData("0101", 1)]
    [InlineData("-5", 1)]
    [InlineData("101", 0)]
    public void Quote_RejectsBadLines(string productId, int quantity)
        => Assert.Equal("invalid_lines", FirstCode(new QuoteWorkspaceOrderValidator(),
            Quote(new WorkspaceInputLine(productId, quantity))));

    [Fact]
    public void Quote_RejectsTheSameProductTwice()
        => Assert.Equal("invalid_lines", FirstCode(new QuoteWorkspaceOrderValidator(),
            Quote(new WorkspaceInputLine("101", 1), new WorkspaceInputLine("101", 2))));

    [Fact]
    public void Quote_RejectsAnEmptyCustomer()
        => Assert.Equal("invalid_lines", FirstCode(new QuoteWorkspaceOrderValidator(),
            new QuoteWorkspaceOrderQuery(new WorkspaceQuoteRequest(" ", [new WorkspaceInputLine("101", 1)]))));

    [Theory]
    [InlineData(0.001)]
    [InlineData(1_000_000.01)]
    [InlineData(-1_000_000.01)]
    public void Quote_RejectsAdjustmentsOutsideWholeKopecksOrTheLimit(double adjustment)
        => Assert.Equal("invalid_price_adjustment", FirstCode(new QuoteWorkspaceOrderValidator(),
            Quote(new WorkspaceInputLine("101", 1, (decimal)adjustment))));

    [Theory]
    [InlineData(" ", "101")]
    [InlineData("customer", "abc")]
    public void CustomerPrices_RejectAMissingCustomerOrBadProduct(string customer, string productId)
        => Assert.Equal("invalid_products", FirstCode(new GetWorkspaceCustomerPricesValidator(),
            new GetWorkspaceCustomerPricesQuery(new WorkspaceCustomerPricesRequest(customer, [productId]))));

    [Fact]
    public void CreateDraft_NeedsAnIdempotencyKeyAndTitle()
    {
        var draft = new WorkspaceDraftInput(" ", null, "customer", [new WorkspaceInputLine("101", 1)]);
        var validator = new CreateWorkspaceDraftValidator();

        Assert.Equal("invalid_idempotency_key", FirstCode(validator, new CreateWorkspaceDraftCommand(draft, Guid.Empty)));
        Assert.Equal("invalid_draft", FirstCode(validator, new CreateWorkspaceDraftCommand(draft, Guid.NewGuid())));
    }

    [Fact]
    public void UpdateAndDeleteDraft_NeedTheIfMatchVersion()
    {
        var draft = new WorkspaceDraftInput("Чернетка", null, "customer", [new WorkspaceInputLine("101", 1)]);

        Assert.Equal("invalid_request", FirstCode(new UpdateWorkspaceDraftValidator(), new UpdateWorkspaceDraftCommand(7, "", draft)));
        Assert.Equal("invalid_request", FirstCode(new DeleteWorkspaceDraftValidator(), new DeleteWorkspaceDraftCommand(7, "")));
        Assert.True(new DeleteWorkspaceDraftValidator().Validate(new DeleteWorkspaceDraftCommand(7, "\"v1\"")).IsValid);
    }

    [Theory]
    [InlineData("2026-03-10", "2026-03-09", "-createdAt", 1, "invalid_date_range")]
    [InlineData(null, null, "price", 1, "invalid_sort")]
    [InlineData(null, null, "-createdAt", 0, "invalid_query")]
    [InlineData("2026-03-09", "2026-03-10", "totalAmount", 1, null)]
    public void OrderList_ChecksDatesSortAndPage(string? from, string? to, string sort, int page, string? code)
        => Assert.Equal(code, FirstCode(new GetWorkspaceOrdersValidator(),
            new GetWorkspaceOrdersQuery(null, null, from, to, sort, page, 20)));
}
