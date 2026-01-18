using Catalog.Domain.ValueObjects;

namespace Catalog.Application.Integrations.OneC.DTOs;

public sealed record CategoryIdAndNameRow(ProductCategoryId Id, string CategoryName);
