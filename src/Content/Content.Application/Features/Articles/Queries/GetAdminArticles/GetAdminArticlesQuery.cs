using Content.Application.Features.Articles.Shared;

namespace Content.Application.Features.Articles.Queries.GetAdminArticles;

/// <summary>All articles, drafts included, newest first; titles in Ukrainian.</summary>
public sealed record GetAdminArticlesQuery : IQuery<Result<IReadOnlyList<AdminArticleListItem>>>;
