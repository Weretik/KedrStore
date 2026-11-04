namespace Media.Infrastructure.Storage;

/// <summary>
/// The CDN that holds library files. The FTP user's home is the CDN root, so keys are paths from the root and the
/// public URL is <see cref="PublicBaseUrl"/> + "/" + key.
/// </summary>
public sealed class MediaStorageOptions
{
    public const string SectionName = "MediaStorage";

    public string PublicBaseUrl { get; set; } = string.Empty;
    public MediaFtpOptions Ftp { get; set; } = new();

    public bool IsConfigured =>
        !string.IsNullOrWhiteSpace(PublicBaseUrl)
        && !string.IsNullOrWhiteSpace(Ftp.Host)
        && !string.IsNullOrWhiteSpace(Ftp.Username)
        && !string.IsNullOrWhiteSpace(Ftp.Password);
}

public sealed class MediaFtpOptions
{
    public string Host { get; set; } = string.Empty;
    public int Port { get; set; } = 21;
    public string Username { get; set; } = string.Empty;
    public string Password { get; set; } = string.Empty;

    /// <summary>FluentFTP encryption mode: Auto tries explicit TLS and falls back to plain FTP.</summary>
    public string Encryption { get; set; } = "Auto";
}
