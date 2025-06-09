namespace Infrastructure.Identity.Configuration;

/// <summary>
/// Клас для зберігання налаштувань адміністративного облікового запису
/// </summary>
public class AdminUserConfig
{
    /// <summary>
    /// Email адміністратора
    /// </summary>
    public string Email { get; set; } = "admin@example.com";

    /// <summary>
    /// Повне ім'я адміністратора
    /// </summary>
    public string FullName { get; set; } = "Адміністратор системи";

    /// <summary>
    /// Пароль за замовчуванням (рекомендується зберігати в секретах, а не в конфігурації)
    /// </summary>
    public string? DefaultPassword { get; set; }

    /// <summary>
    /// Чи заблоковано обліковий запис
    /// </summary>
    public bool LockoutEnabled { get; set; } = false;
}
