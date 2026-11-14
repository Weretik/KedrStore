using K = Scheduling.Application.Features.Catalog.ScheduledTaskKeys;

namespace Scheduling.Application.Features.Catalog;

/// <summary>
/// Every background task of Kedr, in the order of the «Фонові задачі» page (platform/007 R-01). Adding a task: a key
/// here, a runner in Host.Jobs (Tasks/), its secrets on the job in the deploy workflow. The admin cannot add, rename or
/// delete tasks. Tasks of the old Cloud Run jobs start disabled until the switch-over (R-12).
/// </summary>
public static class ScheduledTaskCatalog
{
    public static IReadOnlyList<ScheduledTaskDefinition> Tasks { get; } =
    [
        new(K.Outbox, "Досилання повідомлень",
            "Повторює те, що не пішло з першого разу: швидкі замовлення з сайту в 1С і повідомлення в Telegram.",
            TaskSchedule.Interval(1).Value, MaxMinutes: 5, Editable: false, RunsInTick: true),
        new(K.Stocks, "Залишки з 1С", "Оновлює залишки товарів з 1С: дверей, фурнітури й Cosmos.",
            TaskSchedule.DailyAt(TaskSchedule.EveryDay, new TimeOnly(7, 0), new TimeOnly(13, 0)), 30,
            EnabledByDefault: false),
        new(K.Prices, "Ціни з 1С", "Оновлює типи цін і ціни товарів з 1С.",
            TaskSchedule.DailyAt([DayOfWeek.Monday], new TimeOnly(6, 0)), 30, EnabledByDefault: false),
        new(K.Catalog, "Каталог з 1С",
            "Оновлює категорії й картки товарів з 1С, розпізнає характеристики нових товарів і перебудовує список товарів.",
            TaskSchedule.DailyAt(TaskSchedule.EveryDay, new TimeOnly(3, 0)), 60, EnabledByDefault: false),
        new(K.Customers, "Клієнти з 1С", "Оновлює клієнтів і їхні типи цін з 1С.",
            TaskSchedule.DailyAt(TaskSchedule.EveryDay, new TimeOnly(4, 0)), 30, EnabledByDefault: false),
        new(K.Orders, "Замовлення в 1С",
            "Повторно надсилає в 1С замовлення з адмінки, які не пройшли з першого разу, і повідомляє в Telegram про ті, що не пройшли зовсім.",
            TaskSchedule.Interval(10).Value, 10, EnabledByDefault: false),
        new(K.Media, "Фото з CDN",
            "Додає нові фото з CDN у «Фото», привʼязує їх до товарів і розкладає по папках категорій.",
            TaskSchedule.DailyAt(TaskSchedule.EveryDay, new TimeOnly(5, 0)), 60, EnabledByDefault: false),
        new(K.FullSync, "Повне оновлення з 1С",
            "Усе разом: типи цін, категорії, картки, залишки, ціни, характеристики і список товарів. Лише вручну.",
            TaskSchedule.Manual, 120),
        new(K.ProductList, "Перебудувати список товарів",
            "Перебудовує список товарів сайту й адмінки з того, що вже є в базі. Лише вручну.",
            TaskSchedule.Manual, 30),
    ];

    private static readonly Dictionary<string, ScheduledTaskDefinition> ByKey =
        Tasks.ToDictionary(x => x.Key, StringComparer.Ordinal);

    public static ScheduledTaskDefinition? Find(string key) => ByKey.GetValueOrDefault(key);
}
