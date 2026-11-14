namespace Content.Application.Abstractions.Integrations;

/// <summary>Keeps only the text formatting an article body may use and removes everything else.</summary>
public interface IArticleHtmlSanitizer
{
    string Sanitize(string html);
}
