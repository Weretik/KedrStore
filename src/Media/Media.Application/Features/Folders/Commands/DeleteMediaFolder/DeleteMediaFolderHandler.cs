using Media.Application.Features.Folders.Shared;

namespace Media.Application.Features.Folders.Commands.DeleteMediaFolder;

public sealed class DeleteMediaFolderHandler(IMediaRepository<MediaFolder> folders, IReadMediaDbContext db)
    : ICommandHandler<DeleteMediaFolderCommand, Result>
{
    public const string NotEmptyMessage = "Папка не порожня: спочатку перенесіть або видаліть її файли й підпапки.";

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
