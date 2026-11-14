using Content.Application.Abstractions.Media;
using Content.Application.Features.Articles.Shared;
using Content.Application.Features.Articles.Shared.Reading;

namespace Content.Application.Features.Articles.Commands.UploadArticleCover;

/// <summary>
/// Puts a cover picture into the media library before the article is saved, so a new article gets its cover in the
/// same save; the answer's ID goes into the article input.
/// </summary>
public sealed record UploadArticleCoverCommand(CoverUpload File) : ICommand<Result<UploadedArticleCover>>;
