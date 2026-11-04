namespace Media.Application.Features.Files.Common;

internal sealed class MediaFileByIdSpec : SingleResultSpecification<MediaFile>
{
    public MediaFileByIdSpec(MediaFileId id) => Query.Where(file => file.Id == id);
}

internal sealed class MediaFilesByIdsSpec : Specification<MediaFile>
{
    public MediaFilesByIdsSpec(IReadOnlyCollection<MediaFileId> ids) => Query.Where(file => ids.Contains(file.Id));
}
