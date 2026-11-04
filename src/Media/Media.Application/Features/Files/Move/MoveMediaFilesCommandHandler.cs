using Media.Application.Features.Common;
using Media.Application.Features.Files.Common;
using Media.Application.Features.Folders.Common;

namespace Media.Application.Features.Files.Move;

/// <summary>Moves the files that exist; unknown IDs are ignored, so a stale selection still moves the rest.</summary>
public sealed class MoveMediaFilesCommandHandler(
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
