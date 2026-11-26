using Catalog.Application.Abstractions.Persistence;

namespace Catalog.Application.Features.RetailPrices.Shared;

/// <summary>
/// Saves retail price changes once; a 1C sync that changed the same prices meanwhile makes the <c>xmin</c> check fail,
/// and the admin is asked to try again instead of overwriting it (database rules 4).
/// </summary>
public static class RetailPriceSave
{
    public const string ChangedByOneC = "Ціни саме оновлюються з 1С. Спробуйте ще раз.";

    /// <summary>True when saved; false when another save changed one of the prices first.</summary>
    public static async Task<bool> TrySaveAsync(ICatalogRepository<ProductPrice> prices,
        CancellationToken cancellationToken)
    {
        try
        {
            await prices.SaveChangesAsync(cancellationToken);
            return true;
        }
        catch (DbUpdateConcurrencyException)
        {
            return false;
        }
    }
}
