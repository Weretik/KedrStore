namespace Identity.Application.Features.Avatars;

/// <summary>An uploaded image as the API received it; the infrastructure checks and converts it.</summary>
public sealed record AvatarUpload(Stream Content, long Length);
