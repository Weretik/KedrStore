namespace Content.Application.Features.Articles.Common;

internal sealed class ArticleByIdSpec : SingleResultSpecification<Article>
{
    public ArticleByIdSpec(ArticleId id) => Query.Where(article => article.Id == id);
}

internal sealed class ArticleCoverSpec : SingleResultSpecification<ArticleCover>
{
    public ArticleCoverSpec(ArticleId id) => Query.Where(cover => cover.ArticleId == id);
}
