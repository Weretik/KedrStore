namespace Content.Application.Features.Articles.Shared.Reading;

internal sealed class ArticleByIdSpec : SingleResultSpecification<Article>
{
    public ArticleByIdSpec(ArticleId id) => Query.Where(article => article.Id == id);
}

internal sealed class ArticleCoverSpec : SingleResultSpecification<ArticleCover>
{
    public ArticleCoverSpec(ArticleId id) => Query.Where(cover => cover.ArticleId == id);
}

internal sealed class ArticlesWithLegacyCoverSpec : Specification<Article>
{
    public ArticlesWithLegacyCoverSpec() => Query.Where(article => article.CoverVersion != null).OrderBy(article => article.Id);
}
