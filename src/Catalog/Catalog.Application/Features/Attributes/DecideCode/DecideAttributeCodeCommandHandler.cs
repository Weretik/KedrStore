using Catalog.Application.Contracts.Persistence;
using Catalog.Application.Features.Attributes.Common;
using Catalog.Contracts.Attributes;
using Catalog.Domain.Entities.Attributes;
using Catalog.Domain.ValueObjects;

namespace Catalog.Application.Features.Attributes.DecideCode;

public sealed class DecideAttributeCodeCommandHandler(
    IReadCatalogDbContext db,
    ICatalogRepository<AttributeAlias> aliases,
    UnresolvedCodeReader reader,
    TimeProvider clock)
    : ICommandHandler<DecideAttributeCodeCommand, Result<DecideAttributeCodeResponse>>
{
    public async ValueTask<Result<DecideAttributeCodeResponse>> Handle(DecideAttributeCodeCommand command,
        CancellationToken cancellationToken)
    {
        var aliasId = AttributeAliasId.Create(command.AliasId);
        var alias = await db.AttributeAliases.FirstOrDefaultAsync(item => item.Id == aliasId, cancellationToken);
        if (alias is null)
            return Result.NotFound();

        AttributeValueId? valueId = command.Request.ValueId is { } id ? AttributeValueId.Create(id) : null;
        if (valueId is not null && !await db.AttributeValues.AnyAsync(
                value => value.Id == valueId && value.AttributeId == alias.AttributeId, cancellationToken))
        {
            return AttributeValidation.Invalid("valueId", "The value is not in the code's attribute.", "ValueNotFound");
        }

        var now = clock.GetUtcNow();
        alias.Decide(valueId, now);

        // A product value stays unconfirmed while any code that leads to it is unconfirmed.
        var confirmed = 0;
        if (valueId is not null && !await db.AttributeAliases.AnyAsync(other => other.Id != aliasId
                && other.AttributeId == alias.AttributeId && other.ValueId == valueId && !other.IsConfirmed,
                cancellationToken))
        {
            var waiting = await db.ProductAttributeValues
                .Where(value => value.AttributeId == alias.AttributeId && value.ValueId == valueId && !value.IsConfirmed)
                .ToListAsync(cancellationToken);
            confirmed = waiting.Count(value => value.Confirm(now));
        }

        await aliases.SaveChangesAsync(cancellationToken);
        return Result.Success(new DecideAttributeCodeResponse((await reader.ReadAsync(aliasId, cancellationToken))!,
            confirmed));
    }
}
