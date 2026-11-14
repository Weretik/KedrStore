using Ardalis.Result;
using Sales.Application.Abstractions.Orders;
using Sales.Application.Features.Orders.Queries.GetOrderById;
using Sales.Application.Features.Orders.Queries.GetOrderList;
using Sales.Domain.Orders;

namespace UnitTests.Sales.Application;

public sealed class GetOrderListQueryHandlerTests
{
    [Fact]
    public async Task Handle_ReturnsReaderPageAndPassesScopeAndCancellation()
    {
        var page = new PagedResult<List<OrderListItemDto>>(
            new PagedInfo(2, 5, 3, 11),
            [new OrderListItemDto(7, "SO-7", "cp-1", "Customer", DateTimeOffset.UtcNow, 2, 30m,
                OneCOrderSyncStatus.Accepted, "1C-7")]);
        var reader = new Reader { ListResult = Result.Success(page) };
        using var source = new CancellationTokenSource();
        var request = new OrderListRequest { CounterpartyId = "cp-1", Page = 2, PageSize = 5 };

        var result = await new GetOrderListHandler(reader)
            .Handle(new GetOrderListQuery(request), source.Token);

        Assert.True(result.IsSuccess);
        Assert.Same(page, result.Value);
        Assert.Same(request, reader.ListRequest);
        Assert.Equal(source.Token, reader.CancellationToken);
    }

    [Fact]
    public async Task Handle_PreservesReaderFailure()
    {
        var reader = new Reader { ListResult = Result.Error("read failed") };

        var result = await new GetOrderListHandler(reader)
            .Handle(new GetOrderListQuery(new OrderListRequest()), CancellationToken.None);

        Assert.Equal(ResultStatus.Error, result.Status);
        Assert.Contains("read failed", result.Errors);
    }

    [Theory]
    [InlineData(" ", 1, 20)]
    [InlineData(null, 0, 20)]
    [InlineData(null, 1, 101)]
    public void Validator_RejectsInvalidListInput(string? counterpartyId, int page, int pageSize)
    {
        var result = new GetOrderListValidator().Validate(new GetOrderListQuery(
            new OrderListRequest { CounterpartyId = counterpartyId, Page = page, PageSize = pageSize }));

        Assert.False(result.IsValid);
    }

    private sealed class Reader : IOrderReadService
    {
        public required Result<PagedResult<List<OrderListItemDto>>> ListResult { get; init; }
        public OrderListRequest? ListRequest { get; private set; }
        public CancellationToken CancellationToken { get; private set; }

        public Task<Result<PagedResult<List<OrderListItemDto>>>> GetListAsync(
            OrderListRequest request,
            CancellationToken cancellationToken)
        {
            ListRequest = request;
            CancellationToken = cancellationToken;
            return Task.FromResult(ListResult);
        }

        public Task<Result<OrderDetailDto>> GetByIdAsync(long orderId, CancellationToken cancellationToken)
            => throw new NotSupportedException();
    }
}
