using Media.Application.Features.Folders.Common;

namespace Media.Application.Features.Folders.Delete;

public sealed class DeleteMediaFolderCommandHandler(IMediaRepository<MediaFolder> folders, IReadMediaDbContext db)
    : ICommandHandler<DeleteMediaFolderCommand, Result>
{
    public const string NotEmptyMessage = "The folder is not empty: move or delete its files and subfolders first.";

    public async ValueTask<Result> Handle(DeleteMediaFolderCommand command, CancellationToken cancellationToken)
    {
        var id = MediaFolderId.Create(command.Id);
        var folder = await folders.FirstOrDefaultAsync(new MediaFolderByIdSpec(id), cancellationToken);
        if (folder is null)
            return Result.NotFound();

        var hasContent = await db.MediaFiles.AnyAsync(file => file.FolderId == id, cancellationToken)
                         || await db.MediaFolders.AnyAsync(child => child.ParentId == id, cancellationToken);
        if (hasContent)
            return Result.Conflict(NotEmptyMessage);

        await folders.DeleteAsync(folder, cancellationToken);
        return Result.Success();
    }
}
