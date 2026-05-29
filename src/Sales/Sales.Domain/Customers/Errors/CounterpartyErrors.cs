namespace Sales.Domain.Customers.Errors;

public static class CounterpartyErrors
{
    public static IDomainError IdRequired() =>
        new DomainError("sales.counterparty.id_required", "Counterparty id is required.");

    public static IDomainError IdentityUserIdInvalid(Guid identityUserId) =>
        new DomainError("sales.counterparty.identity_user_id_invalid", $"Identity user id is invalid: {identityUserId}.");

    public static IDomainError NameRequired() =>
        new DomainError("sales.counterparty.name_required", "Counterparty name is required.");

    public static IDomainError NameTooLong(int length) =>
        new DomainError("sales.counterparty.name_too_long", $"Counterparty name is too long: {length}.");

    public static IDomainError EmailRequired() =>
        new DomainError("sales.counterparty.email_required", "Counterparty email is required.");

    public static IDomainError EmailTooLong(int length) =>
        new DomainError("sales.counterparty.email_too_long", $"Counterparty email is too long: {length}.");

    public static IDomainError PriceTypeInvalid(int priceTypeId) =>
        new DomainError("sales.counterparty.price_type_invalid", $"Price type id is invalid: {priceTypeId}.");

    public static IDomainError CategoryInvalid(int categoryId) =>
        new DomainError("sales.counterparty.category_invalid", $"Category id is invalid: {categoryId}.");
}
