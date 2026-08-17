# Identifier strategy

Read this document before creating an entity, changing a primary key, or exposing an identifier through an API or integration. Identifier choice follows the entity lifecycle: where it is created, stored, transmitted, and identified — not a C# convenience default.

## Decision table

| Situation | Default choice |
| --- | --- |
| Small reference data | `int` |
| Ordinary business entity | `long` |
| Very large relational table | `long` |
| One central database | usually `long` |
| Multiple independent writers/services | `Guid` |
| Offline creation | `Guid` |
| Merge of independent databases | `Guid` |
| Event or message identifier | `Guid` |
| Public API identifier | `Guid` or a separate `PublicId` |
| FK-heavy relational model | usually `long` |
| Join table | often a composite key |
| Human-readable document number | separate immutable `string`, never the primary key |

## Decision flow

```text
Need to choose an ID
│
├─ Is it generated in several independent places, offline, or merged from databases?
│  └─ Yes: Guid
│
└─ No: is there one central relational database?
   └─ Yes: long
      └─ Is it a small stable reference table? int

Need to hide the internal database ID in public API, URL, or integration?
└─ Add a separate Guid PublicId; keep the relational PK as long.
```

## Project convention

```csharp
// Domain aggregate/entity: ordinary C# type-safe ID wrapping the selected
// database scalar. Its EF Core column remains bigint.
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

// Small reference data: typed ID wrapping int.
public readonly record struct CurrencyId(int Value);

// Events, messages, independently created/distributed objects: typed Guid ID.
public readonly record struct IntegrationEventId(Guid Value);

// Public exposure when an internal ID must not be shown.
// PublicId is also a typed value object.
Guid PublicId;

// Human document number
string OrderNumber; // unique, immutable, not a PK
```

For every new or changed Domain entity, its ID and Domain foreign keys must use a strongly typed value-object/readonly-struct wrapper over the selected scalar. Never introduce `BaseAuditableEntity<long>`, `BaseEntity<Guid>`, or a raw scalar FK in the Domain. This prevents passing an unrelated ID to an API by accident while retaining efficient database columns and indexes.

Do not use Vogen in this project. Define typed IDs as explicit ordinary C# `readonly record struct` types so their construction, validation, and EF conversion are visible in source code. Existing legacy Vogen usage is outside the scope of a feature unless a separate migration explicitly removes it.

Raw scalar IDs are allowed only inside an Infrastructure persistence-only record that never crosses the Domain boundary, for example an EF idempotency table. Map typed Domain IDs to their scalar EF columns through a value converter.

`OrderNumber` is for people and integrations. It may use a format such as `SO-YYYYMMDD-NNNN`, while the database still uses an efficient numeric primary key.

## Review checklist

Before approving a new entity, answer:

1. Where is it created — one database or multiple independent systems?
2. Does it need offline creation or later database merge?
3. Is it a relational FK-heavy business record or a small reference table?
4. Must its internal identity be hidden from public clients?
5. Does it need a separate human-readable document number?
6. Has the selected scalar been wrapped in a typed Domain ID, and are its Domain foreign keys typed as well?

Do not use `Guid` for every entity merely because it can be generated in application code.
