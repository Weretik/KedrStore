namespace Content.Application.Features.Articles.Commands.MoveLegacyArticleCovers;

/// <summary>
/// Moves every cover saved before Content 008 (bytes in the database) to the CDN folder <c>articles/</c> and the
/// library folder «Статті», then links it to its article. Safe to repeat: moved covers are not found again.
/// </summary>
public sealed record MoveLegacyArticleCoversCommand : ICommand<Result<LegacyArticleCoversMoved>>;

/// <param name="Moved">Covers moved to the CDN by this run; 0 when none were left.</param>
public sealed record LegacyArticleCoversMoved(int Moved);
