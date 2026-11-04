using Media.Application.Features.Common;
using Media.Application.Features.Folders.Common;

namespace Media.Application.Features.Folders.Create;

public sealed class CreateMediaFolderCommandHandler(
    IMediaRepository<MediaFolder> folders,
    IReadMediaDbContext db,
    TimeProvider clock)
    : ICommandHandler<CreateMediaFolderCommand, Result<MediaFolderItem>>
{
    public async ValueTask<Result<MediaFolderItem>> Handle(CreateMediaFolderCommand command,
        CancellationToken cancellationToken)
    {
        var placement = await MediaFolderRules.CheckPlacementAsync(db, command.ParentId, command.Name, null, cancellationToken);
        if (placement is not null)
            return MediaResults.Fail<MediaFolderItem>(placement);

        MediaFolderId? parent = command.ParentId is { } id ? MediaFolderId.Create(id) : null;
        var folder = await folders.AddAsync(MediaFolder.Create(command.Name, parent, clock.GetUtcNow()), cancellationToken);
        return Result.Success(new MediaFolderItem(folder.Id.Value, folder.ParentId?.Value, folder.Name, 0));
    }
}
