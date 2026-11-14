using Catalog.Contracts.Attributes;

namespace Catalog.Application.Features.Attributes.Commands.DecideAttributeCode;

/// <summary>
/// The owner decodes a code: picks a value of its attribute or says it means nothing to filter by. Product values that
/// waited only for this confirmation become confirmed now; codes decoded for the first time reach products at the next
/// recognition run.
/// </summary>
public sealed record DecideAttributeCodeCommand(int AliasId, DecideAttributeCodeRequest Request)
    : ICommand<Result<DecideAttributeCodeResponse>>;
