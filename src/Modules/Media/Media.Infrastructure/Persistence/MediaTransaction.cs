using Media.Application.Abstractions.Persistence;
using Microsoft.EntityFrameworkCore;

namespace Media.Infrastructure.Persistence;

/// <summary>Transaction of the request's <see cref="MediaDbContext"/>, which the Media repositories share.</summary>
internal sealed class MediaTransaction(MediaDbContext dbContext) : IMediaTransaction
{
    public async Task RunAsync(Func<CancellationToken, Task> work, CancellationToken cancellationToken)
    {
        // The in-memory provider of tests has no transactions.
        if (!dbContext.Database.IsRelational() || dbContext.Database.CurrentTransaction is not null)
        {
            await work(cancellationToken);
            return;
        }

        await using var transaction = await dbContext.Database.BeginTransactionAsync(cancellationToken);
        await work(cancellationToken);
        await transaction.CommitAsync(cancellationToken);
    }
}
