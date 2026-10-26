namespace Host.Api.Security.TrustedProxy;

public sealed class TrustedProxyOptions
{
    public const string SectionName = "TrustedProxy";

    public string ClientIpHeaderName { get; init; } = "X-Kedr-Client-IP";
    public string SecretHeaderName { get; init; } = "X-Kedr-Proxy-Secret";
    public string? SharedSecret { get; init; }

    public bool IsEnabled => !string.IsNullOrWhiteSpace(SharedSecret);
}
