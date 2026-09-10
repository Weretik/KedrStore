using Ardalis.Result;
using Sales.Application.Contracts.Customers;
using Sales.Application.Features.Customers.GetById;
using Sales.Application.Features.Customers.GetById.DTOs;
using Sales.Application.Features.Customers.GetById.Validators;
using Sales.Application.Features.Customers.GetList.DTOs;

namespace UnitTests.Sales.Application;

public sealed class GetCustomerByIdQueryHandlerTests
{
    [Fact]
    public async Task Handle_ReturnsExactDetailAndPassesIdentifierAndCancellation()
    {
        var detail = new CustomerDetailDto(
            "customer-1",
            "Customer",
            null,
            "customer@example.com",
            1,
            [new CustomerCategoryPriceTypeDto(10, 20)]);
        var reader = new Reader { DetailResult = Result.Success(detail) };
        using var source = new CancellationTokenSource();

        var result = await new GetCustomerByIdQueryHandler(reader)
            .Handle(new GetCustomerByIdQuery("customer-1"), source.Token);

        Assert.True(result.IsSuccess);
        Assert.Same(detail, result.Value);
        Assert.Equal("customer-1", reader.CounterpartyId);
        Assert.Equal(source.Token, reader.CancellationToken);
    }

    [Fact]
    public async Task Handle_PreservesNotFoundAndReadFailure()
    {
        var missing = await new GetCustomerByIdQueryHandler(new Reader { DetailResult = Result.NotFound() })
            .Handle(new GetCustomerByIdQuery("missing"), CancellationToken.None);
        var failed = await new GetCustomerByIdQueryHandler(new Reader { DetailResult = Result.Error("read failed") })
            .Handle(new GetCustomerByIdQuery("customer-1"), CancellationToken.None);

        Assert.Equal(ResultStatus.NotFound, missing.Status);
        Assert.Equal(ResultStatus.Error, failed.Status);
    }

    [Theory]
    [InlineData("")]
    [InlineData(" ")]
    public void Validator_RejectsBlankIdentifier(string counterpartyId)
    {
        var result = new GetCustomerByIdQueryValidator().Validate(new GetCustomerByIdQuery(counterpartyId));

        Assert.False(result.IsValid);
    }

    [Fact]
    public void Validator_RejectsIdentifierLongerThan64Characters()
    {
        var result = new GetCustomerByIdQueryValidator().Validate(new GetCustomerByIdQuery(new string('x', 65)));

        Assert.False(result.IsValid);
    }

    [Fact]
    public void DetailDto_ContainsOnlyApprovedFields()
    {
        Assert.Equal(
            ["CounterpartyId", "Name", "Phone", "Email", "DefaultPriceTypeId", "CategoryPriceTypes"],
            typeof(CustomerDetailDto).GetProperties().Select(property => property.Name));
    }

    private sealed class Reader : ICustomerReadService
    {
        public required Result<CustomerDetailDto> DetailResult { get; init; }
        public string? CounterpartyId { get; private set; }
        public CancellationToken CancellationToken { get; private set; }

        public Task<Result<PagedResult<List<CustomerListItemDto>>>> GetListAsync(
            CustomerListRequest request,
            CancellationToken cancellationToken)
            => throw new NotSupportedException();

        public Task<Result<CustomerDetailDto>> GetByIdAsync(
            string counterpartyId,
            CancellationToken cancellationToken)
        {
            CounterpartyId = counterpartyId;
            CancellationToken = cancellationToken;
            return Task.FromResult(DetailResult);
        }
    }
}
