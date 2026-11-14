using FluentFTP;
using Media.Application.Abstractions.Storage;
using Microsoft.Extensions.Options;

namespace Media.Infrastructure.Integrations.Storage;

/// <summary>Library files on CDN Express over FTP; the FTP user's home is the CDN root.</summary>
internal sealed class FtpMediaStorage(IOptions<MediaStorageOptions> options) : IMediaStorage
{
    private readonly MediaFtpOptions _ftp = options.Value.Ftp;

    public bool IsConfigured => true;

    public async Task UploadAsync(string key, Stream content, CancellationToken cancellationToken = default)
    {
        await using var client = await ConnectAsync(cancellationToken);
        var status = await client.UploadStream(content, Path(key), FtpRemoteExists.Overwrite, createRemoteDir: true,
            token: cancellationToken);
        if (status != FtpStatus.Success)
            throw new InvalidOperationException($"FTP upload of media file '{key}' returned {status}.");
    }

    public async Task DeleteAsync(string key, CancellationToken cancellationToken = default)
    {
        await using var client = await ConnectAsync(cancellationToken);
        if (await client.FileExists(Path(key), cancellationToken))
            await client.DeleteFile(Path(key), cancellationToken);
    }

    public async Task<IReadOnlyList<StoredMediaObject>> ListAsync(
        IReadOnlyCollection<string> excludedTopDirectories, CancellationToken cancellationToken = default)
    {
        var excluded = excludedTopDirectories.Select(directory => directory.Trim('/'))
            .ToHashSet(StringComparer.OrdinalIgnoreCase);
        await using var client = await ConnectAsync(cancellationToken);

        var result = new List<StoredMediaObject>();
        var pending = new Stack<string>();
        pending.Push("/");
        while (pending.Count > 0)
        {
            var directory = pending.Pop();
            foreach (var item in await client.GetListing(directory, cancellationToken))
            {
                var key = item.FullName.TrimStart('/');
                if (item.Type == FtpObjectType.Directory)
                {
                    if (directory != "/" || !excluded.Contains(item.Name))
                        pending.Push(item.FullName);
                }
                else if (item.Type == FtpObjectType.File)
                {
                    result.Add(new StoredMediaObject(key, item.Size));
                }
            }
        }
        return result;
    }

    private static string Path(string key) => "/" + key.TrimStart('/');

    private async Task<AsyncFtpClient> ConnectAsync(CancellationToken cancellationToken)
    {
        var config = new FtpConfig
        {
            EncryptionMode = Enum.Parse<FtpEncryptionMode>(_ftp.Encryption, ignoreCase: true),
            DataConnectionType = FtpDataConnectionType.AutoPassive,
            ConnectTimeout = 15_000,
            ReadTimeout = 60_000
        };
        var client = new AsyncFtpClient(_ftp.Host, _ftp.Username, _ftp.Password, _ftp.Port, config);
        await client.Connect(cancellationToken);
        return client;
    }
}
