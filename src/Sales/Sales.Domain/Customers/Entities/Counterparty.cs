namespace Sales.Domain.Customers.Entities;

public sealed class Counterparty : BaseAuditableEntity<string>, IAggregateRoot, IAuditableEntity, ISoftDelete
{
    public string Name { get; private set; } = null!;
    public string Email { get; private set; } = null!;
    public string? Phone { get; private set; }
    public int DefaultPriceTypeId { get; private set; }

    private Counterparty() { }

    private Counterparty(
        string id,
        string name,
        string email,
        string? phone,
        int defaultPriceTypeId,
        DateTimeOffset createdAt)
    {
        SetId(id);
        SetName(name);
        SetEmail(email);
        SetPhone(phone);
        SetDefaultPriceTypeId(defaultPriceTypeId);
        MarkAsCreated(createdAt);
    }

    public static Counterparty Create(
        string id,
        string name,
        string email,
        string? phone,
        int defaultPriceTypeId,
        DateTimeOffset createdAt)
        => new(id, name, email, phone, defaultPriceTypeId, createdAt);

    public void Update(
        string name,
        string email,
        string? phone,
        int defaultPriceTypeId,
        DateTimeOffset updatedAt)
    {
        SetName(name);
        SetEmail(email);
        SetPhone(phone);
        SetDefaultPriceTypeId(defaultPriceTypeId);
        MarkAsUpdated(updatedAt);
    }

    private void SetId(string id)
    {
        if (string.IsNullOrWhiteSpace(id))
        {
            throw new DomainException(CounterpartyErrors.IdRequired());
        }

        Id = id.Trim();
    }

    private void SetName(string name)
    {
        if (string.IsNullOrWhiteSpace(name))
        {
            throw new DomainException(CounterpartyErrors.NameRequired());
        }

        var trimmed = name.Trim();
        if (trimmed.Length > 300)
        {
            throw new DomainException(CounterpartyErrors.NameTooLong(trimmed.Length));
        }

        Name = trimmed;
    }

    private void SetEmail(string email)
    {
        if (string.IsNullOrWhiteSpace(email))
        {
            throw new DomainException(CounterpartyErrors.EmailRequired());
        }

        var trimmed = email.Trim();
        if (trimmed.Length > 320)
        {
            throw new DomainException(CounterpartyErrors.EmailTooLong(trimmed.Length));
        }

        Email = trimmed;
    }

    private void SetPhone(string? phone)
    {
        Phone = string.IsNullOrWhiteSpace(phone) ? null : phone.Trim();
    }

    private void SetDefaultPriceTypeId(int defaultPriceTypeId)
    {
        if (defaultPriceTypeId <= 0)
        {
            throw new DomainException(CounterpartyErrors.PriceTypeInvalid(defaultPriceTypeId));
        }

        DefaultPriceTypeId = defaultPriceTypeId;
    }
}
