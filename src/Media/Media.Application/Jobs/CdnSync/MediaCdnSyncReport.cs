namespace Media.Application.Jobs.CdnSync;

/// <summary>What the CDN sync found; with <see cref="DryRun"/> nothing was saved.</summary>
public sealed record MediaCdnSyncReport(
    bool DryRun,
    int FoundOnCdn,
    int AlreadyKnown,
    int Registered,
    int NotPictures,
    int Orphans,
    int MissingOnCdn,
    int HiddenSkipped,
    IReadOnlyDictionary<string, int> RegisteredByFolder);
