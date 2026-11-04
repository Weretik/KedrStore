using Ardalis.Result;
using Content.Application.Contracts.MediaLibrary;

namespace IntegrationTests.TestSupport.Content;

/// <summary>An in-memory media library for Content tests: uploads get IDs from 1000, reads see only known files.</summary>
internal sealed class FakeArticleMediaLibrary : IMediaImageReader, IMediaImageWriter
{
    private readonly Dictionary<long, MediaImage> _files = new();
    private long _nextId = 1000;

    public List<MediaImageUpload> Uploads { get; } = [];

    public long Add(string url)
    {
        var id = ++_nextId;
        _files[id] = new MediaImage(url, $"{id}.webp", 1200, 600);
        return id;
    }

    public Task<IReadOnlyDictionary<long, MediaImage>> GetImagesAsync(IReadOnlyCollection<long> mediaFileIds,
        CancellationToken cancellationToken)
        => Task.FromResult<IReadOnlyDictionary<long, MediaImage>>(mediaFileIds.Where(_files.ContainsKey).Distinct()
            .ToDictionary(id => id, id => _files[id]));

    public Task<Result<StoredMediaImage>> UploadAsync(MediaImageUpload upload, CancellationToken cancellationToken)
    {
        Uploads.Add(upload);
        var id = Add($"https://cdn.example.test/{upload.StorageDirectory}/{upload.FileName}");
        return Task.FromResult(Result.Success(new StoredMediaImage(id, _files[id].Url)));
    }
}
