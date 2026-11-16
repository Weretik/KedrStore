using K = Scheduling.Application.Features.Catalog.ScheduledTaskKeys;

namespace Scheduling.Application.Features.Catalog;

/// <summary>
/// Every background task of Kedr, in the order of the «Фонові задачі» page (platform/007 R-01). Adding a task: a key
/// here, a runner in Host.Jobs (Tasks/), its secrets on the job in the deploy workflow. The admin cannot add, rename or
/// delete tasks. Tasks of the old Cloud Run jobs start disabled until the switch-over (R-12).
/// </summary>
public static class ScheduledTaskCatalog
{
    /// <summary>
    /// Limit of a job task: the job's own timeout. 1C answers slowly, so a long run is not an error (owner, 2026-11-15);
    /// the limit only frees a task whose job was killed or hung.
    /// </summary>
    public const int JobTaskMaxMinutes = 240;

    /// <summary>Tasks that write the catalogue (products, prices, stocks, photos, the product list) one at a time.</summary>
    public const string CatalogGroup = "catalog";

    public static IReadOnlyList<ScheduledTaskDefinition> Tasks { get; } =
    [
        new(K.Outbox, "Досилання повідомлень",
            "Надсилає швидкі замовлення з сайту й повідомлення в Telegram, видаляє акаунти вилучених клієнтів і повторює те, що не пішло з першого разу. Замовлення в 1С йдуть окремою задачею.",
            TaskSchedule.Interval(5).Value, MaxMinutes: 5, Editable: false, RunsInTick: true),
        new(K.OneCOrders, "Відправка замовлень в 1С",
            "Надсилає в 1С замовлення з адмінки й застосунку та повторює ті, що не пішли з першого разу. Поки задача вимкнена, замовлення чекають і не потрапляють у 1С.",
            TaskSchedule.Interval(5).Value, MaxMinutes: 5, RunsInTick: true),
        new(K.Stocks, "Залишки з 1С", "Оновлює залишки товарів з 1С: дверей, фурнітури й Cosmos.",
            TaskSchedule.DailyAt(TaskSchedule.EveryDay, new TimeOnly(7, 0), new TimeOnly(13, 0)), JobTaskMaxMinutes,
            EnabledByDefault: false, Group: CatalogGroup),
        new(K.Prices, "Ціни з 1С", "Оновлює типи цін і ціни товарів з 1С.",
            TaskSchedule.DailyAt([DayOfWeek.Monday], new TimeOnly(6, 0)), JobTaskMaxMinutes,
            EnabledByDefault: false, Group: CatalogGroup),
        new(K.Catalog, "Каталог з 1С",
            "Оновлює категорії й картки товарів з 1С, розпізнає характеристики нових товарів і перебудовує список товарів.",
            TaskSchedule.DailyAt(TaskSchedule.EveryDay, new TimeOnly(3, 0)), JobTaskMaxMinutes,
            EnabledByDefault: false, Group: CatalogGroup),
        new(K.Customers, "Клієнти з 1С", "Оновлює клієнтів і їхні типи цін з 1С.",
            TaskSchedule.DailyAt(TaskSchedule.EveryDay, new TimeOnly(4, 0)), JobTaskMaxMinutes, EnabledByDefault: false),
        new(K.Media, "Фото з CDN",
            "Додає нові фото з CDN у «Фото», привʼязує їх до товарів і розкладає по папках категорій.",
            TaskSchedule.DailyAt(TaskSchedule.EveryDay, new TimeOnly(5, 0)), JobTaskMaxMinutes,
            EnabledByDefault: false, Group: CatalogGroup),
        new(K.FullSync, "Повне оновлення з 1С",
            "Усе разом: типи цін, категорії, картки, залишки, ціни, характеристики і список товарів. Лише вручну.",
            TaskSchedule.Manual, JobTaskMaxMinutes, Group: CatalogGroup),
        new(K.ProductList, "Перебудувати список товарів",
            "Перебудовує список товарів сайту й адмінки з того, що вже є в базі. Лише вручну.",
            TaskSchedule.Manual, JobTaskMaxMinutes, Group: CatalogGroup),
    ];

    private static readonly Dictionary<string, ScheduledTaskDefinition> ByKey =
        Tasks.ToDictionary(x => x.Key, StringComparer.Ordinal);

    public static ScheduledTaskDefinition? Find(string key) => ByKey.GetValueOrDefault(key);
}
