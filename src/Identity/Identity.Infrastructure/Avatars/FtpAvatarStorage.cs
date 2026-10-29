using FluentFTP;
using Identity.Infrastructure.Options;

namespace Identity.Infrastructure.Avatars;

/// <summary>Uploads avatars to CDN Express over FTP; the FTP user is limited to the avatars folder.</summary>
public sealed class FtpAvatarStorage(IOptions<AvatarStorageOptions> options) : IAvatarStorage
{
    private readonly AvatarFtpOptions _ftp = options.Value.Ftp;

    public bool IsConfigured => true;

    public async Task SaveAsync(string key, byte[] content, CancellationToken cancellationToken = default)
    {
        await using var client = await ConnectAsync(cancellationToken);
        var status = await client.UploadBytes(content, "/" + key, FtpRemoteExists.Overwrite, createRemoteDir: true, token: cancellationToken);
        if (status != FtpStatus.Success)
        {
            throw new InvalidOperationException($"FTP upload of avatar '{key}' returned {status}.");
        }
    }

    public async Task DeleteAsync(string key, CancellationToken cancellationToken = default)
    {
        await using var client = await ConnectAsync(cancellationToken);
        if (await client.FileExists("/" + key, cancellationToken))
        {
            await client.DeleteFile("/" + key, cancellationToken);
        }
    }

    private async Task<AsyncFtpClient> ConnectAsync(CancellationToken cancellationToken)
    {
        var config = new FtpConfig
        {
            EncryptionMode = Enum.Parse<FtpEncryptionMode>(_ftp.Encryption, ignoreCase: true),
            DataConnectionType = FtpDataConnectionType.AutoPassive,
            ConnectTimeout = 15_000,
            ReadTimeout = 30_000
        };
        var client = new AsyncFtpClient(_ftp.Host, _ftp.Username, _ftp.Password, _ftp.Port, config);
        await client.Connect(cancellationToken);
        return client;
    }
}
