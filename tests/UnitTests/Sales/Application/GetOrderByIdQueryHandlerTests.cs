using Ardalis.Result;
using Sales.Application.Contracts.Orders;
using Sales.Application.Features.Orders.GetById;
using Sales.Application.Features.Orders.GetById.DTOs;
using Sales.Application.Features.Orders.GetById.Validators;
using Sales.Application.Features.Orders.GetList.DTOs;
using Sales.Domain.Orders.Enums;

namespace UnitTests.Sales.Application;

public sealed class GetOrderByIdQueryHandlerTests
{
    [Fact]
    public async Task Handle_ReturnsExactDetailAndPassesCancellation()
    {
        var detail = new OrderDetailDto(
            42,
            "SO-42",
            DateTimeOffset.UtcNow,
            new OrderCounterpartyDto("cp-1", "Customer", "+380000000000"),
            "Note",
            [new OrderLineDto("p-1", "Product", 2, 25m)],
            25m,
            new OrderSyncDto(OneCOrderSyncStatus.Pending, null, null));
        var reader = new Reader { DetailResult = Result.Success(detail) };
        using var source = new CancellationTokenSource();

        var result = await new GetOrderByIdQueryHandler(reader)
            .Handle(new GetOrderByIdQuery(42), source.Token);

        Assert.True(result.IsSuccess);
        Assert.Same(detail, result.Value);
        Assert.Equal(42, reader.OrderId);
        Assert.Equal(source.Token, reader.CancellationToken);
    }

    [Fact]
    public async Task Handle_PreservesNotFoundAndReadFailure()
    {
        var missing = await new GetOrderByIdQueryHandler(new Reader { DetailResult = Result.NotFound() })
            .Handle(new GetOrderByIdQuery(404), CancellationToken.None);
        var failed = await new GetOrderByIdQueryHandler(new Reader { DetailResult = Result.Error("read failed") })
            .Handle(new GetOrderByIdQuery(42), CancellationToken.None);

        Assert.Equal(ResultStatus.NotFound, missing.Status);
        Assert.Equal(ResultStatus.Error, failed.Status);
    }

    [Theory]
    [InlineData(0)]
    [InlineData(-1)]
    public void Validator_RejectsNonPositiveOrderId(long orderId)
    {
        var result = new GetOrderByIdQueryValidator().Validate(new GetOrderByIdQuery(orderId));

        Assert.False(result.IsValid);
    }

    private sealed class Reader : IOrderReadService
    {
        public required Result<OrderDetailDto> DetailResult { get; init; }
        public long OrderId { get; private set; }
        public CancellationToken CancellationToken { get; private set; }

        public Task<Result<PagedResult<List<OrderListItemDto>>>> GetListAsync(
            OrderListRequest request,
            CancellationToken cancellationToken)
            => throw new NotSupportedException();

        public Task<Result<OrderDetailDto>> GetByIdAsync(long orderId, CancellationToken cancellationToken)
        {
            OrderId = orderId;
            CancellationToken = cancellationToken;
            return Task.FromResult(DetailResult);
        }
    }
}
