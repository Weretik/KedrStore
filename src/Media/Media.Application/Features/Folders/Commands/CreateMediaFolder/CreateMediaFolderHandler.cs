using Media.Application.Features.Shared;
using Media.Application.Features.Folders.Shared;

namespace Media.Application.Features.Folders.Commands.CreateMediaFolder;

public sealed class CreateMediaFolderHandler(
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
        var created = MediaFolder.Create(command.Name, parent, clock.GetUtcNow());
        if (created.IsFailure)
            return created.Error!.ToResult<MediaFolderItem>();

        var folder = await folders.AddAsync(created.Value, cancellationToken);
        return Result.Success(new MediaFolderItem(folder.Id.Value, folder.ParentId?.Value, folder.Name, 0));
    }
}
