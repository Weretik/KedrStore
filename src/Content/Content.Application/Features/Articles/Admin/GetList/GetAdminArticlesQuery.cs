using Content.Application.Features.Articles.Common;

namespace Content.Application.Features.Articles.Admin.GetList;

/// <summary>All articles, drafts included, newest first; titles in Ukrainian.</summary>
public sealed record GetAdminArticlesQuery : IQuery<Result<IReadOnlyList<AdminArticleListItem>>>;
