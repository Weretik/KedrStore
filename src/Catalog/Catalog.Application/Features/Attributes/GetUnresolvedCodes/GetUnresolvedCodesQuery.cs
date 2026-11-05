using Catalog.Contracts.Attributes;

namespace Catalog.Application.Features.Attributes.GetUnresolvedCodes;

/// <summary>The «Не розшифровані коди» tab: codes without a value or with an unconfirmed decoding.</summary>
public sealed record GetUnresolvedCodesQuery : IQuery<Result<List<AdminUnresolvedCodeDto>>>;
