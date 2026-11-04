namespace Content.Application.Features.Articles.Admin.Delete;

/// <summary>Removes an article and its cover for good; to hide it, unpublish instead.</summary>
public sealed record DeleteArticleCommand(long ArticleId) : ICommand<Result>;
