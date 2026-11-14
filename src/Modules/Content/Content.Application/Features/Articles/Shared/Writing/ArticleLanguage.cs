namespace Content.Application.Features.Articles.Shared.Writing;

public static class ArticleLanguage
{
    public static bool IsSupported(string? language) => language is not null && Article.Languages.Contains(language);
}
