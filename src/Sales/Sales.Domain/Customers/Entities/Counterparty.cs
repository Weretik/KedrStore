namespace Sales.Domain.Customers.Entities;

public sealed class Counterparty : BaseAuditableEntity<string>, IAggregateRoot, IAuditableEntity, ISoftDelete
{
    public const int NameMaxLength = 300;
    public const int EmailMaxLength = 320;

    public Guid IdentityUserId { get; private set; }
    public string Name { get; private set; } = null!;
    public string Email { get; private set; } = null!;
    public string? Phone { get; private set; }
    public int DefaultPriceTypeId { get; private set; }

    private Counterparty() { }

    public static DomainResult<Counterparty> Create(
        string id,
        Guid identityUserId,
        string name,
        string email,
        string? phone,
        int defaultPriceTypeId,
        DateTimeOffset createdAt)
    {
        if (string.IsNullOrWhiteSpace(id))
            return CounterpartyErrors.IdRequired();

        var counterparty = new Counterparty { Id = id.Trim() };
        var set = counterparty.Set(identityUserId, name, email, phone, defaultPriceTypeId);
        if (set.IsFailure)
            return set.Error!;

        counterparty.MarkAsCreated(createdAt);
        return counterparty;
    }

    public DomainResult Update(
        Guid identityUserId,
        string name,
        string email,
        string? phone,
        int defaultPriceTypeId,
        DateTimeOffset updatedAt)
    {
        var set = Set(identityUserId, name, email, phone, defaultPriceTypeId);
        if (set.IsFailure)
            return set;

        MarkAsUpdated(updatedAt);
        return DomainResult.Success();
    }

    public void Restore(DateTimeOffset updatedAt)
    {
        if (!IsDeleted)
            return;

        IsDeleted = false;
        MarkAsUpdated(updatedAt);
    }

    private DomainResult Set(Guid identityUserId, string name, string email, string? phone, int defaultPriceTypeId)
    {
        if (identityUserId == Guid.Empty)
            return CounterpartyErrors.IdentityUserIdInvalid(identityUserId);
        if (string.IsNullOrWhiteSpace(name))
            return CounterpartyErrors.NameRequired();
        var trimmedName = name.Trim();
        if (trimmedName.Length > NameMaxLength)
            return CounterpartyErrors.NameTooLong(trimmedName.Length);
        if (string.IsNullOrWhiteSpace(email))
            return CounterpartyErrors.EmailRequired();
        var trimmedEmail = email.Trim();
        if (trimmedEmail.Length > EmailMaxLength)
            return CounterpartyErrors.EmailTooLong(trimmedEmail.Length);
        if (defaultPriceTypeId <= 0)
            return CounterpartyErrors.PriceTypeInvalid(defaultPriceTypeId);

        IdentityUserId = identityUserId;
        Name = trimmedName;
        Email = trimmedEmail;
        Phone = string.IsNullOrWhiteSpace(phone) ? null : phone.Trim();
        DefaultPriceTypeId = defaultPriceTypeId;
        return DomainResult.Success();
    }
}
