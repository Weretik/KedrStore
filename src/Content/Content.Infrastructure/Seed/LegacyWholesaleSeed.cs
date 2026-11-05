using System.Text.Json;
using Microsoft.EntityFrameworkCore.Migrations;

namespace Content.Infrastructure.Seed;

/// <summary>
/// The texts of the page «Співпраця» that the site kept in code (kedr-web <c>wholesale-dictionary.ts</c>); used once
/// by a data migration, so the page looks the same right after the deploy. JSON keys match the EF mapping.
/// </summary>
internal static class LegacyWholesaleSeed
{
    private static readonly DateTimeOffset SeededAt = new(2026, 11, 5, 0, 0, 0, TimeSpan.Zero);

    private static readonly object Uk = new
    {
        HeroTitle = "Дверна фурнітура оптом",
        HeroLead = "Завіси, замки, ручки, циліндри та міжкімнатні механізми для ваших закупівель. Обирайте товари, а оптові умови уточнюйте у менеджера Class Kedr.",
        ContactCardTitle = "Обговорімо ваше замовлення",
        ContactCardNote = "Наявність і деталі закупівлі — у менеджера.",
        AssortmentTitle = "Асортимент дверної фурнітури",
        AssortmentIntro = "Оберіть розділ і підготуйте список товарів для запиту.",
        CompanyTitle = "фурнітура для вашого бізнесу",
        CompanyText = "Постачаємо дверну фурнітуру для магазинів, виробників дверей і роздрібних покупців. Допомагаємо зорієнтуватися в асортименті та погодити потрібні позиції.",
        SelectionTitle = "Підбір фурнітури",
        SelectionText = "враховуємо тип дверей і технічні параметри.",
        WarehouseTitle = "Складська програма",
        WarehouseText = "наявність потрібних позицій уточнює менеджер.",
        StepsTitle = "Як замовити фурнітуру оптом",
        Steps = new[]
        {
            new { Title = "Оберіть товари", Text = "Знайдіть позиції в каталозі." },
            new { Title = "Надішліть запит", Text = "Артикули або назви й кількість." },
            new { Title = "Узгодьте замовлення", Text = "Деталі закупівлі з менеджером." },
        },
        FaqTitle = "Питання оптових покупців",
        Faq = new[]
        {
            new { Question = "Як дізнатися оптові ціни та умови?", Answer = "Зверніться до менеджера телефоном, у Telegram або Viber." },
            new { Question = "Що підготувати для звернення?", Answer = "Артикули або назви товарів, орієнтовну кількість і місто доставки." },
            new { Question = "Чи допомагаєте з підбором фурнітури?", Answer = "Так. Підготуйте тип дверей, розміри або фото потрібної деталі." },
        },
        ClosingTitle = "Обговорімо вашу закупівлю",
    };

    private static readonly object Ru = new
    {
        HeroTitle = "Дверная фурнитура оптом",
        HeroLead = "Петли, замки, ручки, цилиндры и межкомнатные механизмы для ваших закупок. Выбирайте товары, а оптовые условия уточняйте у менеджера Class Kedr.",
        ContactCardTitle = "Обсудим ваш заказ",
        ContactCardNote = "Наличие и детали закупки — у менеджера.",
        AssortmentTitle = "Ассортимент дверной фурнитуры",
        AssortmentIntro = "Выберите раздел и подготовьте список товаров для запроса.",
        CompanyTitle = "фурнитура для вашего бизнеса",
        CompanyText = "Поставляем дверную фурнитуру для магазинов, производителей дверей и розничных покупателей. Помогаем сориентироваться в ассортименте и согласовать нужные позиции.",
        SelectionTitle = "Подбор фурнитуры",
        SelectionText = "учитываем тип дверей и технические параметры.",
        WarehouseTitle = "Складская программа",
        WarehouseText = "наличие нужных позиций уточняет менеджер.",
        StepsTitle = "Как заказать фурнитуру оптом",
        Steps = new[]
        {
            new { Title = "Выберите товары", Text = "Найдите позиции в каталоге." },
            new { Title = "Отправьте запрос", Text = "Артикулы или названия и количество." },
            new { Title = "Согласуйте заказ", Text = "Детали закупки с менеджером." },
        },
        FaqTitle = "Вопросы оптовых покупателей",
        Faq = new[]
        {
            new { Question = "Как узнать оптовые цены и условия?", Answer = "Свяжитесь с менеджером по телефону, в Telegram или Viber." },
            new { Question = "Что подготовить для обращения?", Answer = "Артикулы или названия товаров, примерное количество и город доставки." },
            new { Question = "Помогаете с подбором фурнитуры?", Answer = "Да. Подготовьте тип дверей, размеры или фото нужной детали." },
        },
        ClosingTitle = "Обсудим вашу закупку",
    };

    public static void Up(MigrationBuilder migrationBuilder) => migrationBuilder.Sql(
        $"""
         INSERT INTO content."WholesalePages" ("Id", "Uk", "Ru", "UpdatedAt")
         VALUES ({WholesalePage.SingletonId}, {Json(Uk)}::jsonb, {Json(Ru)}::jsonb, '{SeededAt:O}');
         """);

    public static void Down(MigrationBuilder migrationBuilder) =>
        migrationBuilder.Sql($"""DELETE FROM content."WholesalePages" WHERE "Id" = {WholesalePage.SingletonId};""");

    private static string Json(object value)
        => "'" + JsonSerializer.Serialize(value, new JsonSerializerOptions
        {
            Encoder = System.Text.Encodings.Web.JavaScriptEncoder.UnsafeRelaxedJsonEscaping,
        }).Replace("'", "''", StringComparison.Ordinal) + "'";
}
