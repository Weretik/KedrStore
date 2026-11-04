namespace Media.Application.Features.Folders.Common;

internal sealed class MediaFolderByIdSpec : SingleResultSpecification<MediaFolder>
{
    public MediaFolderByIdSpec(MediaFolderId id) => Query.Where(folder => folder.Id == id);
}
