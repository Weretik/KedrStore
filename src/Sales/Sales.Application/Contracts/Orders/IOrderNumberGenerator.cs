namespace Sales.Application.Contracts.Orders;

public interface IOrderNumberGenerator
{
    Task<string> GenerateAsync(DateTimeOffset createdAtUtc, CancellationToken cancellationToken);
}
