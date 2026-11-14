namespace Media.Application.Features.Files.Commands.DeleteMediaFile;

/// <summary>Deletes the file from the CDN and the library (R-6).</summary>
public sealed record DeleteMediaFileCommand(long Id) : ICommand<Result>;
