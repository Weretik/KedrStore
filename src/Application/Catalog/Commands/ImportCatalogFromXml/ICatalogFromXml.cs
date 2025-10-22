namespace Application.Catalog.Commands.ImportCatalogFromXml;

public interface ICatalogFromXml
{
    string FileName { get; }
    string? ContentType { get; }
    long FileSize { get; }
    Stream Content { get; }
}
