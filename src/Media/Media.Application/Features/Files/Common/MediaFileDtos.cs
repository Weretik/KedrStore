namespace Media.Application.Features.Files.Common;

/// <summary>A library file with its public CDN URL.</summary>
public sealed record MediaFileItem(
    long Id,
    long? FolderId,
    string FileName,
    string Url,
    string ContentType,
    long SizeBytes,
    int? Width,
    int? Height,
    DateTimeOffset CreatedAt,
    DateTimeOffset UpdatedAt);

/// <summary>One page of files and the total number of files that match the filter.</summary>
public sealed record MediaFilePage(IReadOnlyList<MediaFileItem> Items, int Page, int PageSize, int TotalCount);
