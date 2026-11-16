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
            "Що робить: надсилає з черги швидкі замовлення з сайту, повідомлення в Telegram і видалення акаунтів клієнтів, які попросили видалити дані. Якщо щось не пішло з першого разу, пробує ще раз.\n" +
            "Коли: постійно, кожні кілька хвилин; розклад не змінюється.\n" +
            "Якщо вимкнено: нічого не зламається, але все з черги чекатиме, поки задачу не запустять. Замовлення в 1С сюди не входять: їх надсилає окрема задача.",
            TaskSchedule.Interval(5).Value, MaxMinutes: 5, Editable: false, RunsInTick: true),
        new(K.OneCOrders, "Відправка замовлень в 1С",
            "Що робить: надсилає в 1С замовлення, оформлені в адмінці й у застосунку менеджера. Якщо 1С не відповіла, пробує ще раз.\n" +
            "Коли: кожні кілька хвилин, щоб замовлення швидко потрапляли в 1С.\n" +
            "Якщо вимкнено: замовлення зберігаються в Кедрі й чекають, але в 1С не потрапляють, поки задачу не ввімкнуть.",
            TaskSchedule.Interval(5).Value, MaxMinutes: 5, RunsInTick: true),
        new(K.Stocks, "Залишки з 1С",
            "Що робить: завантажує з 1С актуальні залишки на складах для дверей, фурнітури й Cosmos.\n" +
            "Коли: двічі на день, зранку й удень, щоб на сайті й у застосунку були свіжі залишки.\n" +
            "Якщо вимкнено: залишки не оновлюються й залишаються такими, якими були під час останнього запуску.",
            TaskSchedule.DailyAt(TaskSchedule.EveryDay, new TimeOnly(7, 0), new TimeOnly(13, 0)), JobTaskMaxMinutes,
            EnabledByDefault: false, Group: CatalogGroup),
        new(K.Prices, "Ціни з 1С",
            "Що робить: завантажує з 1С типи цін і ціни товарів, за якими бачать ціни клієнти й менеджери.\n" +
            "Коли: раз на тиждень, бо ціни змінюються рідко. Якщо змінили ціни раніше, запустіть вручну.\n" +
            "Якщо вимкнено: ціни на сайті й в адмінці не змінюються, доки їх не оновлять уручну.",
            TaskSchedule.DailyAt([DayOfWeek.Monday], new TimeOnly(6, 0)), JobTaskMaxMinutes,
            EnabledByDefault: false, Group: CatalogGroup),
        new(K.Catalog, "Каталог з 1С",
            "Що робить: завантажує з 1С категорії й картки товарів (назви, артикули, описи), сам визначає характеристики нових товарів і перебудовує список товарів сайту й адмінки.\n" +
            "Коли: щоночі, поки ніхто не працює з каталогом.\n" +
            "Якщо вимкнено: нові й змінені в 1С товари не зʼявляться в Кедрі, поки каталог не оновлять вручну.",
            TaskSchedule.DailyAt(TaskSchedule.EveryDay, new TimeOnly(3, 0)), JobTaskMaxMinutes,
            EnabledByDefault: false, Group: CatalogGroup),
        new(K.Customers, "Клієнти з 1С",
            "Що робить: завантажує з 1С клієнтів і призначені їм типи цін, щоб клієнт бачив у кабінеті свої ціни.\n" +
            "Коли: щоночі.\n" +
            "Якщо вимкнено: нові клієнти з 1С і зміни їхніх типів цін не зʼявляться в Кедрі.",
            TaskSchedule.DailyAt(TaskSchedule.EveryDay, new TimeOnly(4, 0)), JobTaskMaxMinutes, EnabledByDefault: false),
        new(K.Media, "Фото з CDN",
            "Що робить: знаходить на CDN нові фото, додає їх у розділ «Фото», привʼязує до товарів за артикулом і розкладає по папках категорій.\n" +
            "Коли: щоночі, після оновлення каталогу.\n" +
            "Якщо вимкнено: нові фото не зʼявляться на товарах, поки їх не підтягнуть вручну.",
            TaskSchedule.DailyAt(TaskSchedule.EveryDay, new TimeOnly(5, 0)), JobTaskMaxMinutes,
            EnabledByDefault: false, Group: CatalogGroup),
        new(K.FullSync, "Повне оновлення з 1С",
            "Що робить: за один запуск робить усе разом: типи цін, категорії, картки, залишки, ціни, характеристики й список товарів. Довга задача, використовуйте після великих змін в 1С.\n" +
            "Коли: лише коли натиснули «Запустити зараз», сама не запускається.\n" +
            "Якщо вимкнено: нічого, окремі задачі працюють як завжди.",
            TaskSchedule.Manual, JobTaskMaxMinutes, Group: CatalogGroup),
        new(K.ProductList, "Перебудувати список товарів",
            "Що робить: збирає наново список товарів сайту й адмінки з того, що вже є в базі, без звернення до 1С. Допомагає, якщо список показує застарілі дані.\n" +
            "Коли: лише коли натиснули «Запустити зараз», сама не запускається.\n" +
            "Якщо вимкнено: нічого, список оновлюється під час звичайних задач.",
            TaskSchedule.Manual, JobTaskMaxMinutes, Group: CatalogGroup),
    ];

    private static readonly Dictionary<string, ScheduledTaskDefinition> ByKey =
        Tasks.ToDictionary(x => x.Key, StringComparer.Ordinal);

    public static ScheduledTaskDefinition? Find(string key) => ByKey.GetValueOrDefault(key);
}
