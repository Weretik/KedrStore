namespace Sales.Application.Abstractions.Orders;

public interface IOrderNumberGenerator
{
    Task<string> GenerateAsync(DateTimeOffset createdAtUtc, CancellationToken cancellationToken);
}
