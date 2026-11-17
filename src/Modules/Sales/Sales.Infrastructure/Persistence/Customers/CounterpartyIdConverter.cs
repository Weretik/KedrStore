using Microsoft.EntityFrameworkCore.Storage.ValueConversion;

namespace Sales.Infrastructure.Persistence.Customers;

/// <summary>Stores a <see cref="CounterpartyId"/> as its 1C code text.</summary>
internal sealed class CounterpartyIdConverter() : ValueConverter<CounterpartyId, string>(
    id => id.Value,
    value => CounterpartyId.FromStorage(value));
