using Identity.Infrastructure.Options;

namespace Identity.Infrastructure.Avatars;

/// <summary>Builds the public CDN URL at read time, so a host change needs no data change.</summary>
public sealed class AvatarUrlBuilder(IOptions<AvatarStorageOptions> options)
{
    private readonly AvatarStorageOptions _options = options.Value;

    public string? Build(string? avatarPath)
    {
        if (string.IsNullOrWhiteSpace(avatarPath) || string.IsNullOrWhiteSpace(_options.PublicBaseUrl))
        {
            return null;
        }

        var prefix = _options.PublicPathPrefix.Trim('/');
        var path = prefix.Length == 0 ? avatarPath : $"{prefix}/{avatarPath}";
        return $"{_options.PublicBaseUrl.TrimEnd('/')}/{path}";
    }
}
