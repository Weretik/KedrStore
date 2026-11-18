namespace Content.Domain.Home;

/// <summary>
/// A brand on the home page: its name (the logo's alternative text) and a logo from the media library, stored as a
/// file ID without a cross-module key. The site shows brands by <see cref="Position"/>, smallest first.
/// </summary>
public sealed class HomeBrand : IAggregateRoot
{
    public const int NameMaxLength = 100;

    public HomeBrandId Id { get; private set; }
    public int Position { get; private set; }
    public string Name { get; private set; } = string.Empty;
    public MediaFileId LogoMediaFileId { get; private set; }
    public DateTimeOffset CreatedAt { get; private set; }
    public DateTimeOffset UpdatedAt { get; private set; }

    private HomeBrand() { }

    public static DomainResult<HomeBrand> Create(string name, MediaFileId logoMediaFileId, int position, DateTimeOffset now)
    {
        var checkedPosition = HomeGuard.Position(position);
        if (checkedPosition.IsFailure)
            return checkedPosition.Error!;

        var brand = new HomeBrand { Position = checkedPosition.Value, CreatedAt = now };
        var updated = brand.Update(name, logoMediaFileId, now);
        return updated.IsSuccess ? brand : updated.Error!;
    }

    public DomainResult Update(string name, MediaFileId logoMediaFileId, DateTimeOffset now)
    {
        if (logoMediaFileId.Value <= 0)
            return HomeErrors.MediaFileInvalid(logoMediaFileId.Value);
        var checkedName = HomeGuard.Text(name, nameof(Name), NameMaxLength);
        if (checkedName.IsFailure)
            return checkedName;

        Name = checkedName.Value;
        LogoMediaFileId = logoMediaFileId;
        UpdatedAt = now;
        return DomainResult.Success();
    }

    public DomainResult MoveTo(int position, DateTimeOffset now)
    {
        var checkedPosition = HomeGuard.Position(position);
        if (checkedPosition.IsFailure)
            return checkedPosition;
        if (checkedPosition.Value == Position)
            return DomainResult.Success();

        Position = position;
        UpdatedAt = now;
        return DomainResult.Success();
    }
}
