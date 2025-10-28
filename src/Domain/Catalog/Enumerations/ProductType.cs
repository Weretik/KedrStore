namespace Domain.Catalog.Enumerations;

public sealed class ProductType(string name, int value) : SmartEnum<ProductType>(name, value)
{
    public static readonly ProductType Doors = new("Двері", 1);
    public static readonly ProductType DoorHardware = new("Дверна фурнітура", 2);
}
