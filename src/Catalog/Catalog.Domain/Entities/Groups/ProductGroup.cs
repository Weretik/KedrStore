using Catalog.Domain.Errors;
using Catalog.Domain.ValueObjects;

namespace Catalog.Domain.Entities.Groups;

/// <summary>
/// Variants of one model in one category (owner decision 2026-11-05): each color, size, and side stays a separate
/// product; the group only links them for one listing card and the variant switchers. An automatic group is found
/// by its key (normalized base name) in its category; a group a person created has no key.
/// </summary>
public class ProductGroup : IAggregateRoot
{
    public const int NameMaxLength = 300;
    public const int SlugMaxLength = 100;

    public ProductGroupId Id { get; private set; }
    public ProductCategoryId CategoryId { get; private set; }
    public string Name { get; private set; } = null!;
    /// <summary>Normalized base name of an automatic group; <c>null</c> for a group a person created.</summary>
    public string? GroupKey { get; private set; }
    public string Slug { get; private set; } = null!;
    public ProductId? MainProductId { get; private set; }
    /// <summary>A person chose the main product; automation keeps it while it is on the site.</summary>
    public bool IsMainManual { get; private set; }
    public bool IsCreatedManually { get; private set; }
    public DateTimeOffset UpdatedAt { get; private set; }

    private ProductGroup() { }

    public static ProductGroup CreateAutomatic(ProductCategoryId categoryId, string name, string groupKey, string slug,
        DateTimeOffset now)
        => new()
        {
            CategoryId = categoryId,
            Name = Text(name, NameMaxLength, nameof(Name)),
            GroupKey = Text(groupKey, NameMaxLength, nameof(GroupKey)),
            Slug = Text(slug, SlugMaxLength, nameof(Slug)),
            UpdatedAt = now
        };

    public static ProductGroup CreateManual(ProductCategoryId categoryId, string name, string slug, DateTimeOffset now)
        => new()
        {
            CategoryId = categoryId,
            Name = Text(name, NameMaxLength, nameof(Name)),
            Slug = Text(slug, SlugMaxLength, nameof(Slug)),
            IsCreatedManually = true,
            UpdatedAt = now
        };

    public bool Rename(string name, DateTimeOffset now)
    {
        var value = Text(name, NameMaxLength, nameof(Name));
        if (value == Name)
            return false;

        Name = value;
        UpdatedAt = now;
        return true;
    }

    public void ChangeSlug(string slug, DateTimeOffset now)
    {
        Slug = Text(slug, SlugMaxLength, nameof(Slug));
        UpdatedAt = now;
    }

    /// <summary>Automation picks the main product; a choice made by a person is dropped. Returns whether it changed.</summary>
    public bool SetMainAutomatically(ProductId? productId, DateTimeOffset now)
    {
        if (MainProductId == productId && !IsMainManual)
            return false;

        MainProductId = productId;
        IsMainManual = false;
        UpdatedAt = now;
        return true;
    }

    public void SetMainManually(ProductId productId, DateTimeOffset now)
    {
        MainProductId = productId;
        IsMainManual = true;
        UpdatedAt = now;
    }

    private static string Text(string? value, int maxLength, string field)
    {
        var trimmed = value?.Trim();
        if (string.IsNullOrEmpty(trimmed))
            throw new DomainException(ProductGroupErrors.Required(field));
        if (trimmed.Length > maxLength)
            throw new DomainException(ProductGroupErrors.TooLong(field, maxLength, trimmed.Length));

        return trimmed;
    }
}
