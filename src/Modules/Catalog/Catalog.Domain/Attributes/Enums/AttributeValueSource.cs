namespace Catalog.Domain.Attributes.Enums;

/// <summary>Who set a product attribute value. Manual values are never overwritten automatically.</summary>
public enum AttributeValueSource : short
{
    Import = 1,
    Parser = 2,
    Manual = 3
}
