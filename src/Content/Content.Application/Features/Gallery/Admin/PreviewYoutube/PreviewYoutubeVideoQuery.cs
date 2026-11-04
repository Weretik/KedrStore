using Content.Application.Features.Gallery.Common;

namespace Content.Application.Features.Gallery.Admin.PreviewYoutube;

/// <summary>Resolves a pasted link to a video ID, poster and title before the manager adds it.</summary>
public sealed record PreviewYoutubeVideoQuery(string? Url) : IQuery<Result<YoutubePreview>>;
