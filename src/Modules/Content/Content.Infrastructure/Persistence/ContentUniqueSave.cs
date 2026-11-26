using BuildingBlocks.Infrastructure.Persistence;

namespace Content.Infrastructure.Persistence;

/// <summary>Saves the scoped content store; a broken unique index (a racing duplicate) gives <c>false</c>.</summary>
internal sealed class ContentUniqueSave(ContentDbContext db) : IContentUniqueSave
{
    public async Task<bool> TrySaveChangesAsync(CancellationToken cancellationToken)
    {
        try
        {
            await db.SaveChangesAsync(cancellationToken);
            return true;
        }
        catch (DbUpdateException error) when (UniqueViolation.Is(error))
        {
            return false;
        }
    }
}
