using Media.Application.Features.Shared;
using Media.Application.Features.Files.Shared;
using Media.Application.Features.Folders.Shared;

namespace Media.Application.Features.Files.Commands.MoveMediaFiles;

/// <summary>Moves the files that exist; unknown IDs are ignored, so a stale selection still moves the rest.</summary>
public sealed class MoveMediaFilesHandler(
    IMediaRepository<MediaFile> files,
    IReadMediaDbContext db,
    TimeProvider clock)
    : ICommandHandler<MoveMediaFilesCommand, Result<int>>
{
    public async ValueTask<Result<int>> Handle(MoveMediaFilesCommand command, CancellationToken cancellationToken)
    {
        var missing = await MediaFolderRules.CheckExistsAsync(db, command.FolderId, "folderId", cancellationToken);
        if (missing is not null)
            return MediaResults.Fail<int>(missing);

        var ids = command.FileIds.Distinct().Select(MediaFileId.Create).ToArray();
        var found = await files.ListAsync(new MediaFilesByIdsSpec(ids), cancellationToken);
        MediaFolderId? folder = command.FolderId is { } id ? MediaFolderId.Create(id) : null;
        var now = clock.GetUtcNow();
        foreach (var file in found)
            file.MoveTo(folder, now);
        await files.SaveChangesAsync(cancellationToken);
        return Result.Success(found.Count);
    }
}
