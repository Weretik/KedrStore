namespace Content.Domain.Home;

/// <summary>
/// A brand on the home page: its name (the logo's alternative text) and a logo from the media library, stored as a
/// file ID without a cross-module key. The site shows brands by <see cref="Position"/>, smallest first.
/// </summary>
public sealed class HomeBrand : IAggregateRoot
{
    public const int NameMaxLength = 100;

    public long Id { get; private set; }
    public int Position { get; private set; }
    public string Name { get; private set; } = string.Empty;
    public long LogoMediaFileId { get; private set; }
    public DateTimeOffset CreatedAt { get; private set; }
    public DateTimeOffset UpdatedAt { get; private set; }

    private HomeBrand() { }

    public static HomeBrand Create(string name, long logoMediaFileId, int position, DateTimeOffset now)
    {
        var brand = new HomeBrand { Position = HomeGuard.Position(position), CreatedAt = now };
        brand.Update(name, logoMediaFileId, now);
        return brand;
    }

    public void Update(string name, long logoMediaFileId, DateTimeOffset now)
    {
        if (logoMediaFileId <= 0)
            throw new DomainException(HomeErrors.MediaFileInvalid(logoMediaFileId));
        Name = HomeGuard.Text(name, nameof(Name), NameMaxLength);
        LogoMediaFileId = logoMediaFileId;
        UpdatedAt = now;
    }

    public void MoveTo(int position, DateTimeOffset now)
    {
        if (HomeGuard.Position(position) == Position)
            return;
        Position = position;
        UpdatedAt = now;
    }
}
