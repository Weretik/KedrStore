using Ardalis.Result;
using Catalog.Application.Abstractions.Persistence;

namespace Catalog.Infrastructure.Persistence;

/// <summary>Transaction of the request's <see cref="CatalogDbContext"/>, which the Catalog repositories share.</summary>
internal sealed class CatalogTransaction(CatalogDbContext dbContext) : ICatalogTransaction
{
    public async Task<Result<T>> RunAsync<T>(Func<CancellationToken, Task<Result<T>>> work,
        CancellationToken cancellationToken)
    {
        // The in-memory provider of tests has no transactions.
        if (!dbContext.Database.IsRelational() || dbContext.Database.CurrentTransaction is not null)
            return await work(cancellationToken);

        await using var transaction = await dbContext.Database.BeginTransactionAsync(cancellationToken);
        var result = await work(cancellationToken);
        if (result.IsSuccess)
            await transaction.CommitAsync(cancellationToken);
        return result;
    }
}
