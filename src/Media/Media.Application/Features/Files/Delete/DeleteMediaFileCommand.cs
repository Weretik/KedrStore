namespace Media.Application.Features.Files.Delete;

/// <summary>Deletes the file from the CDN and the library (R-6).</summary>
public sealed record DeleteMediaFileCommand(long Id) : ICommand<Result>;
