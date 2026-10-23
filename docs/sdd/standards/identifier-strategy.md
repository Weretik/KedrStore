# Identifier strategy

Read this document before creating an entity, changing a primary key, or exposing an identifier through an API or integration. Choose an ID based on the entity lifecycle—where it is created, stored, transmitted, and identified—not on the default convenience of a C# type.

## Decision table

| Situation | Typical choice |
| --- | --- |
| Small lookup table | `int` |
| Ordinary business entity | `long` |
| Very large relational table | `long` |
| One central database | usually `long` |
| Multiple independent writers/services | `Guid` |
| Offline creation | `Guid` |
| Merging independent databases | `Guid` |
| Event or message ID | `Guid` |
| Public API identifier | `Guid` or separate `PublicId` |
| Relational model with many FKs | usually `long` |
| Join table | often a composite key |
| Human-readable document number | separate immutable `string`, never a primary key |

## Selection flow

```text
Choose an ID
│
├─ Generated in multiple independent places, offline, or while merging databases?
│  └─ Yes: Guid
│
└─ No: one central relational database?
   └─ Yes: long
      └─ Small stable lookup table? int

Need to hide the internal database ID in a public API, URL, or integration?
└─ Add a separate Guid PublicId; keep the relational PK long.
```

## Project rule

```csharp
// A regular typed C# ID wraps the chosen database scalar in the Domain aggregate/entity.
// The EF Core column remains bigint.
public readonly record struct OrderId
{
    public long Value { get; }

    public static OrderId Create(long value)
    {
        if (value <= 0)
            throw new ArgumentOutOfRangeException(nameof(value));

        return new OrderId(value);
    }

    private OrderId(long value) => Value = value;
}

public sealed class Order : BaseAuditableEntity<OrderId>;

// Domain foreign keys use the same typed ID.
public OrderId OrderId { get; private set; }

// Small lookup: typed ID wrapping int.
public readonly record struct CurrencyId(int Value);

// Events, messages, and independently/distributed-created objects: typed Guid ID.
public readonly record struct IntegrationEventId(Guid Value);

// Public ID when the internal ID should not be exposed.
// PublicId is also a typed value object.
Guid PublicId;

// Human-readable document number
string OrderNumber; // unique, immutable, not the PK
```

For every new or changed Domain entity, its ID and Domain foreign keys must use a typed value-object/readonly-struct wrapper around the chosen scalar. Do not add `BaseAuditableEntity<long>`, `BaseEntity<Guid>`, or raw scalar FKs to Domain. This prevents accidental use of another ID in APIs while retaining efficient database columns and indexes.

Do not use Vogen for new IDs in this project. Declare typed IDs as explicit regular C# `readonly record struct` types so creation, validation, and EF conversion remain visible in code. Existing legacy Vogen usage is out of feature scope unless a separate migration explicitly removes it.

Raw scalar IDs are allowed only in internal Infrastructure persistence records that do not cross the Domain boundary, such as an EF idempotency table. Map typed Domain IDs to scalar EF columns through value converters.

`OrderNumber` is intended for people and integrations. It may use a format such as `SO-YYYYMMDD-NNNN`, while the database stores an efficient numeric primary key.

## Review checklist

Before approving a new entity, answer:

1. Where is it created—in one database or multiple independent systems?
2. Is offline creation or later database merging required?
3. Is it a business record with many FKs or a small lookup table?
4. Must the internal ID be hidden from public clients?
5. Is a separate human-readable document number needed?
6. Is the chosen scalar wrapped in a typed Domain ID, with its foreign keys typed the same way?

Do not use `Guid` for every entity just because application code can generate one.
