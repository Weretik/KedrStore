using BuildingBlocks.Domain.Abstractions.Errors;

namespace Sales.Domain.Customers;

public static class CounterpartyErrors
{
    public static DomainError IdRequired() =>
        new("sales.counterparty.id_required", "Потрібен код контрагента.", Field: "CounterpartyId");

    public static DomainError IdentityUserIdInvalid(Guid identityUserId) =>
        new("sales.counterparty.identity_user_id_invalid", $"Некоректний користувач контрагента: {identityUserId}.");

    public static DomainError NameRequired() =>
        new("sales.counterparty.name_required", "Потрібна назва контрагента.", Field: "Name");

    public static DomainError NameTooLong(int length) =>
        new("sales.counterparty.name_too_long", $"Назва контрагента задовга ({length} символів).", Field: "Name");

    public static DomainError EmailRequired() =>
        new("sales.counterparty.email_required", "Потрібна електронна пошта контрагента.", Field: "Email");

    public static DomainError EmailTooLong(int length) =>
        new("sales.counterparty.email_too_long", $"Електронна пошта контрагента задовга ({length} символів).", Field: "Email");

    public static DomainError PriceTypeInvalid(int priceTypeId) =>
        new("sales.counterparty.price_type_invalid", $"Некоректний тип ціни: {priceTypeId}.", Field: "PriceTypeId");

    public static DomainError CategoryInvalid(int categoryId) =>
        new("sales.counterparty.category_invalid", $"Некоректна категорія: {categoryId}.", Field: "CategoryId");
}
