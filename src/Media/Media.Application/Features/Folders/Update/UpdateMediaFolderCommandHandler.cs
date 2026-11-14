using Media.Application.Features.Common;
using Media.Application.Features.Folders.Common;

namespace Media.Application.Features.Folders.Update;

public sealed class UpdateMediaFolderCommandHandler(
    IMediaRepository<MediaFolder> folders,
    IReadMediaDbContext db,
    TimeProvider clock)
    : ICommandHandler<UpdateMediaFolderCommand, Result<MediaFolderItem>>
{
    public async ValueTask<Result<MediaFolderItem>> Handle(UpdateMediaFolderCommand command,
        CancellationToken cancellationToken)
    {
        var id = MediaFolderId.Create(command.Id);
        var folder = await folders.FirstOrDefaultAsync(new MediaFolderByIdSpec(id), cancellationToken);
        if (folder is null)
            return Result.NotFound();

        if (await MediaFolderRules.IsSelfOrDescendantAsync(db, id, command.ParentId, cancellationToken))
            return Result.Invalid(new ValidationError("parentId", MediaErrors.FolderMoveIntoItself().Message));

        var placement = await MediaFolderRules.CheckPlacementAsync(db, command.ParentId, command.Name, command.Id, cancellationToken);
        if (placement is not null)
            return MediaResults.Fail<MediaFolderItem>(placement);

        var now = clock.GetUtcNow();
        var changed = DomainResult.Combine(
            folder.Rename(command.Name, now),
            folder.MoveTo(command.ParentId is { } parent ? MediaFolderId.Create(parent) : null, now));
        if (changed.IsFailure)
            return changed.Error!.ToResult<MediaFolderItem>();
        await folders.SaveChangesAsync(cancellationToken);

        var fileCount = await db.MediaFiles.CountAsync(file => file.FolderId == id, cancellationToken);
        return Result.Success(new MediaFolderItem(folder.Id.Value, folder.ParentId?.Value, folder.Name, fileCount));
    }
}
