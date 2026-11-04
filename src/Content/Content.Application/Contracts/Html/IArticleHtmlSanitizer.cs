namespace Content.Application.Contracts.Html;

/// <summary>Keeps only the text formatting an article body may use and removes everything else.</summary>
public interface IArticleHtmlSanitizer
{
    string Sanitize(string html);
}
