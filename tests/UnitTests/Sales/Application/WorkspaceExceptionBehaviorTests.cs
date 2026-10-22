using Ardalis.Result;
using BuildingBlocks.Application.Behaviors;
using Microsoft.Extensions.Logging.Abstractions;
using Sales.Application.Contracts.Orders;
using Sales.Application.Features.Orders.AdminWorkspace.Orders.Quote;

namespace UnitTests.Sales.Application;

public sealed class WorkspaceExceptionBehaviorTests
{
    [Fact]
    public async Task ExpectedWorkspaceError_ReachesApiBoundaryWithOriginalStatusAndCode()
    {
        var expected = new WorkspaceException(400, "unavailable_product", "Недостатньо товару.");
        var behavior = new ExceptionBehavior<QuoteWorkspaceOrderQuery, Result<WorkspaceQuote>>(
            NullLogger<ExceptionBehavior<QuoteWorkspaceOrderQuery, Result<WorkspaceQuote>>>.Instance);
        var query = new QuoteWorkspaceOrderQuery(new WorkspaceQuoteRequest("customer", []));

        var actual = await Assert.ThrowsAsync<WorkspaceException>(() => behavior.Handle(
            query,
            (_, _) => ValueTask.FromException<Result<WorkspaceQuote>>(expected),
            CancellationToken.None).AsTask());

        Assert.Same(expected, actual);
        Assert.Equal(400, actual.StatusCode);
        Assert.Equal("unavailable_product", actual.Code);
    }
}
