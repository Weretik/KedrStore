using Media.Application.Features.Files.Shared;

namespace Media.Application.Features.Files.Queries.GetMediaFile;

public sealed record GetMediaFileQuery(long Id) : IQuery<Result<MediaFileItem>>;
