namespace Domain.Catalog.Enumerations;

public sealed class PriceType(string name, int value, string title) : SmartEnum<PriceType>(name, value)
{
    public string Title { get; } = title;

    public static readonly PriceType KrDv = new("price_1", 1, "Кр.Дв");
    public static readonly PriceType Purchase = new("price_2", 2,"Закуп");
    public static readonly PriceType Vip = new("price_3", 3,"VIP");
    public static readonly PriceType SuperWholesale = new("price_4", 4,"Супер опт");
    public static readonly PriceType LargeWholesaleUah = new("price_5", 5,"Великий опт грн");
    public static readonly PriceType LargeWholesale = new("price_6", 6,"Великий опт usd");
    public static readonly PriceType WholesaleUsd = new("price_7", 7,"Опт usd");
    public static readonly PriceType WholesaleGrn = new("price_11", 11,"Опт");
    public static readonly PriceType SmallWholesaleUsd = new("price_8", 8,"Дрібний опт usd");
    public static readonly PriceType SmallWholesaleGrn = new("price_9", 9,"Дрібний опт грн");
    public static readonly PriceType Retail = new("price_10", 10,"Роздріб");
    public static readonly PriceType Retail30Percent= new("price_12", 12,"Роздріб (халтмаг) 30%");
    public static readonly PriceType KyivRetailUsd   = new("price_13", 13, "Київ роздріб usd");
    public static readonly PriceType KyivRetailUah    = new("price_14", 14, "Київ роздріб грн");

}

