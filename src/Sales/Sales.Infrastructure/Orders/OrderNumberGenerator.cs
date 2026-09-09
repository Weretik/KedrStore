using Sales.Application.Contracts.Orders;

namespace Sales.Infrastructure.Orders;

internal sealed class OrderNumberGenerator : IOrderNumberGenerator
{
    public Task<string> GenerateAsync(DateTimeOffset createdAtUtc, CancellationToken cancellationToken)
        => Task.FromResult($"P{Guid.NewGuid():N}"[..32]);
}
