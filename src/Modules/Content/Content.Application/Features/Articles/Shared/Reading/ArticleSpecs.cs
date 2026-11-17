namespace Content.Application.Features.Articles.Shared.Reading;

internal sealed class ArticleByIdSpec : SingleResultSpecification<Article>
{
    public ArticleByIdSpec(ArticleId id) => Query.Where(article => article.Id == id);
}

/// <summary>The article with its legacy cover bytes, for the commands that change the cover.</summary>
internal sealed class ArticleWithLegacyCoverByIdSpec : SingleResultSpecification<Article>
{
    public ArticleWithLegacyCoverByIdSpec(ArticleId id)
        => Query.Where(article => article.Id == id).Include(article => article.LegacyCover);
}

internal sealed class ArticlesWithLegacyCoverSpec : Specification<Article>
{
    public ArticlesWithLegacyCoverSpec()
        => Query.Where(article => article.CoverVersion != null).Include(article => article.LegacyCover).OrderBy(article => article.Id);
}
