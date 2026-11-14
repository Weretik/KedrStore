namespace Scheduling.Application.Features.Catalog;

/// <summary>Stable task keys: rows in the database and the names the job runs (--tasks).</summary>
public static class ScheduledTaskKeys
{
    public const string Outbox = "outbox";
    public const string Stocks = "stocks";
    public const string Prices = "prices";
    public const string Catalog = "catalog";
    public const string Customers = "customers";
    public const string Orders = "orders";
    public const string Media = "media";
    public const string FullSync = "full-sync";
    public const string ProductList = "product-list";
}
