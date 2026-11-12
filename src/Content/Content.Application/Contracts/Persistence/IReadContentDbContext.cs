namespace Content.Application.Contracts.Persistence;

/// <summary>Read access for queries; writes go through <see cref="IContentRepository{T}"/>.</summary>
public interface IReadContentDbContext
{
    DbSet<Article> Articles { get; }
    DbSet<ArticleCover> ArticleCovers { get; }
    DbSet<CompanyContacts> CompanyContacts { get; }
    DbSet<CompanyRequisites> CompanyRequisites { get; }
    DbSet<CompanyRequisitesChange> CompanyRequisitesChanges { get; }
    DbSet<LegalDocumentVersion> LegalDocumentVersions { get; }
    DbSet<GalleryItem> GalleryItems { get; }
    DbSet<HomeFaqItem> HomeFaqItems { get; }
    DbSet<HomeBrand> HomeBrands { get; }
    DbSet<WholesalePage> WholesalePages { get; }
    DbSet<PageSeo> PageSeo { get; }
    DbSet<SiteSeoSettings> SiteSeoSettings { get; }
}
