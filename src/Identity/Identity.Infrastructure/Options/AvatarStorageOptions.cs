namespace Identity.Infrastructure.Options;

/// <summary>
/// Avatar files on the CDN. The FTP user's access directory is the avatars folder,
/// so file keys are relative to it and the public URL adds <see cref="PublicPathPrefix"/>.
/// </summary>
public sealed class AvatarStorageOptions
{
    public const string SectionName = "AvatarStorage";

    public string PublicBaseUrl { get; set; } = string.Empty;
    public string PublicPathPrefix { get; set; } = "avatars";
    public AvatarFtpOptions Ftp { get; set; } = new();

    public bool IsConfigured =>
        !string.IsNullOrWhiteSpace(PublicBaseUrl)
        && !string.IsNullOrWhiteSpace(Ftp.Host)
        && !string.IsNullOrWhiteSpace(Ftp.Username)
        && !string.IsNullOrWhiteSpace(Ftp.Password);
}

public sealed class AvatarFtpOptions
{
    public string Host { get; set; } = string.Empty;
    public int Port { get; set; } = 21;
    public string Username { get; set; } = string.Empty;
    public string Password { get; set; } = string.Empty;

    /// <summary>FluentFTP encryption mode: Auto tries explicit TLS and falls back to plain FTP.</summary>
    public string Encryption { get; set; } = "Auto";
}
