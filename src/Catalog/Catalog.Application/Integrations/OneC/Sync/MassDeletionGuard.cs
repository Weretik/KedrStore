namespace Catalog.Application.Integrations.OneC.Sync;

/// <summary>Blocks a run that would soft-delete more than 30% of the active rows of a root (R-004).</summary>
public static class MassDeletionGuard
{
    public const double MaxDeletedShare = 0.30;

    public static bool Allows(int activeCount, int toDelete)
        => toDelete == 0 || toDelete <= activeCount * MaxDeletedShare;
}
