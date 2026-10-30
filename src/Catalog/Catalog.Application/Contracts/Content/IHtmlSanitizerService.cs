namespace Catalog.Application.Contracts.Content;

/// <summary>Cleans Admin-written HTML down to the category-text allow-list (R-013).</summary>
public interface IHtmlSanitizerService
{
    string Sanitize(string html);
}
