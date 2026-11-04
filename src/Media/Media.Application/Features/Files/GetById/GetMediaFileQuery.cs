using Media.Application.Features.Files.Common;

namespace Media.Application.Features.Files.GetById;

public sealed record GetMediaFileQuery(long Id) : IQuery<Result<MediaFileItem>>;
