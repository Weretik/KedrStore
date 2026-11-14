namespace Media.Application.Features.Files.Commands.MoveMediaFiles;

/// <summary>Moves several files into <paramref name="FolderId"/> (<c>null</c> = library root); returns how many moved.</summary>
public sealed record MoveMediaFilesCommand(IReadOnlyList<long> FileIds, long? FolderId) : ICommand<Result<int>>;
