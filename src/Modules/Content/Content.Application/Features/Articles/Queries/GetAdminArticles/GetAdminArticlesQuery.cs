using Content.Application.Features.Articles.Shared;
using Content.Application.Features.Articles.Shared.Reading;

namespace Content.Application.Features.Articles.Queries.GetAdminArticles;

/// <summary>All articles, drafts included, newest first; titles in Ukrainian.</summary>
public sealed record GetAdminArticlesQuery : IQuery<Result<IReadOnlyList<AdminArticleListItem>>>;
