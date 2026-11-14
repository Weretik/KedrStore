namespace Catalog.Domain.Attributes.Enums;

/// <summary>How an attribute stores its values.</summary>
public enum AttributeKind : short
{
    /// <summary>A value from the attribute's list (color, side, material).</summary>
    Enum = 1,

    /// <summary>A number in the attribute's unit (length in mm).</summary>
    Number = 2,

    /// <summary>A size expression from the list, for example «100×75×2.5 mm».</summary>
    Size = 3
}
