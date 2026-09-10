using Ardalis.Result;
using Sales.Application.Contracts.Customers;
using Sales.Application.Features.Customers.GetById.DTOs;
using Sales.Application.Features.Customers.GetList;
using Sales.Application.Features.Customers.GetList.DTOs;
using Sales.Application.Features.Customers.GetList.Validators;

namespace UnitTests.Sales.Application;

public sealed class GetCustomerListQueryHandlerTests
{
    [Fact]
    public async Task Handle_ReturnsReaderPageAndPassesRequestAndCancellation()
    {
        var page = new PagedResult<List<CustomerListItemDto>>(
            new PagedInfo(2, 5, 3, 11),
            [new CustomerListItemDto("customer-1", "Customer", null)]);
        var reader = new Reader { ListResult = Result.Success(page) };
        using var source = new CancellationTokenSource();
        var request = new CustomerListRequest { Page = 2, PageSize = 5 };

        var result = await new GetCustomerListQueryHandler(reader)
            .Handle(new GetCustomerListQuery(request), source.Token);

        Assert.True(result.IsSuccess);
        Assert.Same(page, result.Value);
        Assert.Same(request, reader.ListRequest);
        Assert.Equal(source.Token, reader.CancellationToken);
    }

    [Fact]
    public async Task Handle_PreservesReaderFailure()
    {
        var reader = new Reader { ListResult = Result.Error("read failed") };

        var result = await new GetCustomerListQueryHandler(reader)
            .Handle(new GetCustomerListQuery(new CustomerListRequest()), CancellationToken.None);

        Assert.Equal(ResultStatus.Error, result.Status);
        Assert.Contains("read failed", result.Errors);
    }

    [Theory]
    [InlineData(0, 20)]
    [InlineData(1, 0)]
    [InlineData(1, 101)]
    public void Validator_RejectsInvalidPaging(int page, int pageSize)
    {
        var result = new GetCustomerListQueryValidator().Validate(new GetCustomerListQuery(
            new CustomerListRequest { Page = page, PageSize = pageSize }));

        Assert.False(result.IsValid);
    }

    private sealed class Reader : ICustomerReadService
    {
        public required Result<PagedResult<List<CustomerListItemDto>>> ListResult { get; init; }
        public CustomerListRequest? ListRequest { get; private set; }
        public CancellationToken CancellationToken { get; private set; }

        public Task<Result<PagedResult<List<CustomerListItemDto>>>> GetListAsync(
            CustomerListRequest request,
            CancellationToken cancellationToken)
        {
            ListRequest = request;
            CancellationToken = cancellationToken;
            return Task.FromResult(ListResult);
        }

        public Task<Result<CustomerDetailDto>> GetByIdAsync(
            string counterpartyId,
            CancellationToken cancellationToken)
            => throw new NotSupportedException();
    }
}
