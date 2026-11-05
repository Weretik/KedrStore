using Catalog.Application.Attributes.Recognition;
using Catalog.Application.Contracts.Persistence;
using Catalog.Application.Features.Attributes.Common;
using Catalog.Application.Jobs.Attributes;
using Catalog.Contracts.Attributes;
using Catalog.Domain.Entities.Attributes;
using Catalog.Domain.ValueObjects;

namespace Catalog.Application.Features.Attributes.DecideCode;

public sealed class DecideAttributeCodeCommandHandler(
    IReadCatalogDbContext db,
    ICatalogRepository<AttributeAlias> aliases,
    UnresolvedCodeReader reader,
    RecognizeCatalogAttributesJob recognizer,
    TimeProvider clock,
    ILogger<DecideAttributeCodeCommandHandler> logger)
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
        await RecognizeAgainAsync(alias.RawValue, cancellationToken);
        return Result.Success(new DecideAttributeCodeResponse((await reader.ReadAsync(aliasId, cancellationToken))!,
            confirmed));
    }

    /// <summary>
    /// The decision reaches the products at once: those whose names contain the code are recognized again. A failure
    /// here keeps the decision; the next 1C sync recognizes them.
    /// </summary>
    private async Task RecognizeAgainAsync(string rawValue, CancellationToken cancellationToken)
    {
        try
        {
            var names = await db.Products.IgnoreQueryFilters().AsNoTracking()
                .Select(product => new { product.Id, product.Name })
                .ToListAsync(cancellationToken);
            var ids = names.Where(product => ProductAttributeRecognizer.Mentions(product.Name, rawValue))
                .Select(product => product.Id.Value)
                .ToList();
            if (ids.Count > 0)
                await recognizer.RunAsync(apply: true, ids, cancellationToken);
        }
        catch (Exception exception) when (exception is not OperationCanceledException)
        {
            logger.LogError(exception, "Recognizing products with code {RawValue} after the owner's decision failed.",
                rawValue);
        }
    }
}
