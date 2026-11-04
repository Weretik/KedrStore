using Content.Application.Contracts.Images;
using Content.Application.Features.Articles.Common;

namespace Content.Application.Features.Articles.Admin.UploadCover;

public sealed record UploadArticleCoverCommand(long ArticleId, CoverUpload File) : ICommand<Result<AdminArticleDetails>>;
