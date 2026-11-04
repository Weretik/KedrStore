namespace Content.Application.Contracts.Persistence;

/// <summary>Read access for queries; writes go through <see cref="IContentRepository{T}"/>.</summary>
public interface IReadContentDbContext
{
    DbSet<Article> Articles { get; }
    DbSet<ArticleCover> ArticleCovers { get; }
    DbSet<CompanyContacts> CompanyContacts { get; }
}
