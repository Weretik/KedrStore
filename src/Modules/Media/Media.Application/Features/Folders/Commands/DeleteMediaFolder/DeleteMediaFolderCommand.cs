namespace Media.Application.Features.Folders.Commands.DeleteMediaFolder;

/// <summary>Deletes an empty folder: no files and no subfolders.</summary>
public sealed record DeleteMediaFolderCommand(long Id) : ICommand<Result>;
