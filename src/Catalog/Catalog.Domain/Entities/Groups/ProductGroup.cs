using Catalog.Domain.Enums;
using Catalog.Domain.Errors;
using Catalog.Domain.ValueObjects;

namespace Catalog.Domain.Entities.Groups;

/// <summary>
/// Variants of one model in one category (owner decision 2026-11-05): each color, size, and side stays a separate
/// product; the group only links them for one listing card and the variant switchers. An automatic group is found
/// by its key (normalized base name) in its category; a group a person created has no key.
/// Since spec catalog/007 a group is the site <b>model</b>: one site page with its axes and texts, proposed by
/// automation and accepted by a person.
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
    /// <summary>Proposed by automation or accepted by a person (spec catalog/007).</summary>
    public ProductGroupStatus Status { get; private set; }
    public DateTimeOffset? AcceptedAt { get; private set; }
    public string? AcceptedBy { get; private set; }
    public DateTimeOffset UpdatedAt { get; private set; }

    public bool IsAccepted => Status == ProductGroupStatus.Accepted;
    public const int AcceptedByMaxLength = 256;

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
            Status = ProductGroupStatus.Accepted,
            AcceptedAt = now,
            UpdatedAt = now
        };

    /// <summary>
    /// A person accepts the model. Its base-name key is dropped, so automation proposes new products with the same base
    /// name as a new model instead of adding them here. Returns whether it changed.
    /// </summary>
    public bool Accept(string acceptedBy, DateTimeOffset now)
    {
        if (IsAccepted)
            return false;

        Status = ProductGroupStatus.Accepted;
        GroupKey = null;
        AcceptedAt = now;
        AcceptedBy = acceptedBy.Length <= AcceptedByMaxLength ? acceptedBy : acceptedBy[..AcceptedByMaxLength];
        UpdatedAt = now;
        return true;
    }

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
