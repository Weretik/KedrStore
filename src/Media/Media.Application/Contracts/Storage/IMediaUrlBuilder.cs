namespace Media.Application.Contracts.Storage;

/// <summary>Builds the public CDN URL at read time, so a host change needs no data change.</summary>
public interface IMediaUrlBuilder
{
    string Build(string storageKey);
}
