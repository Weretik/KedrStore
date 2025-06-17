namespace Application.Common.Errors;

public static class AppErrorDescriptions
{
    public static class System
    {
        public const string NotFound = "Ресурс не знайдено.";
        public const string Unexpected = "Неочікувана помилка.";
        public const string Persistence = "Помилка збереження або доступу до даних: ";
        public const string RateLimitExceeded = "Перевищено ліміт запитів.";
        public const string Unknown = "Невідома системна помилка.";
    }

    public static class Auth
    {
        public const string Unauthorized = "Неавторизований доступ.";
        public const string Forbidden = "Доступ заборонено.";
        public const string UserNotFound = "Користувача не знайдено: ";
        public const string EmailAlreadyExists = "Користувач з такою електронною поштою вже існує: ";
        public const string TokenInvalid = "Недійсний токен доступу: ";
        public const string TokenExpired = "Термін дії токена закінчився: ";
    }

    public static class Domain
    {
        //public const string Violation = "Порушення бізнес-логіки.";
        public const string Integration = "Помилка під час інтеграції з зовнішнім сервісом: ";
        public const string Payment = "Помилка обробки платежу.";
        public const string Inventory = "Недостатньо товарів на складі.";
    }

    public static class Catalog
    {
        public const string ProductNotFound = "товар не знайдено.";
        public const string CategoryNotFound = "категорію не знайдено.";
        public const string ProductAlreadyExists = "Такий товар вже існує.";
        public const string CategoryAlreadyExists = "Категорія вже існує.";
    }

    public static class Order
    {
        public const string NotFound = "Замовлення не знайдено.";
        public const string AlreadyExists = "Таке замовлення вже існує.";
        public const string ProcessingError = "Помилка обробки замовлення.";
        public const string CancellationError = "Не вдалося скасувати замовлення.";
    }

    public static class Storage
    {
        public const string FileUploadFailed = "Не вдалося завантажити файл: ";
        public const string FileNotFound = "файл не знайдено.";
    }

    public static class Service
    {
        public const string ExternalServiceError = "Зовнішній сервіс повернув помилку: ";
    }

    public static class Cache
    {
        public const string Miss = "Кеш не містить потрібного значення.";
        public const string InvalidationFailed = "Не вдалося записати в кеш.";
    }
}
