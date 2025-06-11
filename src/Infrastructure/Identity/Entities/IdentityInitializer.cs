using Domain.Identity.Interfaces;
using Infrastructure.Identity.Constants;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

namespace Infrastructure.Identity.Entities;

/// <summary>
/// Ініціалізатор даних для Identity
/// </summary>
public class IdentityInitializer : IInitializer
{
    /// <summary>
    /// Ініціалізує початкові дані для Identity
    /// </summary>
    /// <param name="serviceProvider">Провайдер сервісів для отримання необхідних залежностей</param>
    public async Task InitializeAsync(IServiceProvider? serviceProvider, CancellationToken cancellationToken = default)
    {
        try
        {
            var roleManager = serviceProvider.GetRequiredService<RoleManager<AppRole>>();
            var userManager = serviceProvider.GetRequiredService<UserManager<AppUser>>();
            var configuration = serviceProvider.GetRequiredService<IConfiguration>();
            var logger = serviceProvider.GetRequiredService<ILogger<IdentityInitializer>>();

            // Створюємо ролі
            await CreateRolesAsync(roleManager, logger, cancellationToken);

            // Створюємо адміністратора
            await CreateAdminUserAsync(userManager, roleManager, configuration, logger, cancellationToken);
        }
        catch (Exception ex)
        {
            var logger = serviceProvider.GetRequiredService<ILogger<IdentityInitializer>>();
            logger.LogError(ex, "Помилка при ініціалізації даних Identity");
            throw;
        }
    }
    private static async Task CreateRolesAsync(RoleManager<AppRole> roleManager, ILogger logger, CancellationToken cancellationToken)
    {
        // Перевіряємо наявність ролей
        if (!await roleManager.Roles.AnyAsync(cancellationToken))
        {
            logger.LogInformation("Створення ролей за замовчуванням");

            // Список ролей для створення
            var roles = new[]
            {
                new AppRole
                {
                    Name = AppRoles.Administrator,
                    NormalizedName = AppRoles.Administrator.ToUpperInvariant(),
                    Description = "Адміністратор системи з повним доступом",
                    Scope = "system",
                    IsSystemRole = true,
                    AccessLevel = 100
                },
                new AppRole
                {
                    Name = AppRoles.Manager,
                    NormalizedName = AppRoles.Manager.ToUpperInvariant(),
                    Description = "Менеджер з доступом до керування контентом",
                    Scope = "content",
                    IsSystemRole = true,
                    AccessLevel = 50
                },
                new AppRole
                {
                    Name = AppRoles.User,
                    NormalizedName = AppRoles.User.ToUpperInvariant(),
                    Description = "Звичайний користувач системи",
                    Scope = "user",
                    IsSystemRole = true,
                    AccessLevel = 10
                }
            };

            // Створюємо ролі
            foreach (var role in roles)
            {
                var result = await roleManager.CreateAsync(role);
                if (!result.Succeeded)
                {
                    var errors = string.Join(", ", result.Errors.Select(e => e.Description));
                    logger.LogError("Помилка при створенні ролі {RoleName}: {Errors}", role.Name, errors);
                }
                else
                {
                    logger.LogInformation("Роль {RoleName} успішно створена", role.Name);
                }
            }
        }
    }

    private static async Task CreateAdminUserAsync(UserManager<AppUser> userManager, RoleManager<AppRole> roleManager,
        IConfiguration configuration, ILogger logger, CancellationToken cancellationToken)
    {
        // Отримання налаштувань з конфігурації або змінних середовища
        var adminSettings = configuration.GetSection("Identity:AdminUser");
        var adminEmail = adminSettings["Email"] ?? Environment.GetEnvironmentVariable("ADMIN_EMAIL") ?? "admin@example.com";

        // Перевіряємо наявність адміністратора
        if (await userManager.FindByEmailAsync(adminEmail) == null)
        {
            logger.LogInformation("Створення користувача-адміністратора");

            // Отримання безпечного паролю із конфігурації або змінних середовища
            var defaultPassword = adminSettings["DefaultPassword"] ??
                                  Environment.GetEnvironmentVariable("ADMIN_DEFAULT_PASSWORD") ??
                                  GenerateSecurePassword();

            // Створюємо користувача
            var admin = new AppUser
            {
                UserName = adminEmail,
                Email = adminEmail,
                EmailConfirmed = true,
                FullName = adminSettings["FullName"] ?? "Адміністратор системи",
                LockoutEnabled = false
            };

            var result = await userManager.CreateAsync(admin, defaultPassword);
            if (!result.Succeeded)
            {
                var errors = string.Join(", ", result.Errors.Select(e => e.Description));
                logger.LogError("Помилка при створенні адміністратора: {Errors}", errors);
                return;
            }

            // Якщо пароль був згенерований автоматично, виводимо його в лог тільки в dev середовищі
            if (Environment.GetEnvironmentVariable("ASPNETCORE_ENVIRONMENT") == "Development" &&
                adminSettings["DefaultPassword"] == null &&
                Environment.GetEnvironmentVariable("ADMIN_DEFAULT_PASSWORD") == null)
            {
                logger.LogWarning("Згенеровано тимчасовий пароль для адміністратора: {Password}. Змініть його після першого входу!", defaultPassword);
            }

            // Призначаємо роль адміністратора через UserRole для збереження додаткових властивостей
            var adminRole = await roleManager.FindByNameAsync(AppRoles.Administrator);
            if (adminRole != null)
            {
                var userRole = new AppUserRole
                {
                    UserId = admin.Id,
                    RoleId = adminRole.Id,
                    AssignedAt = DateTime.UtcNow,
                    AssignedByUserId = admin.Id.ToString(), // самопризначення для першого користувача
                    IsTemporary = false,
                    Notes = "Автоматично створена роль адміністратора при ініціалізації системи"
                };

                // Використовуємо стандартний метод додавання ролі, внутрішні дані будуть додані автоматично
                result = await userManager.AddToRoleAsync(admin, AppRoles.Administrator);
                if (!result.Succeeded)
                {
                    var errors = string.Join(", ", result.Errors.Select(e => e.Description));
                    logger.LogError("Помилка при призначенні ролі адміністратора: {Errors}", errors);
                    return;
                }
            }

            logger.LogInformation("Користувач-адміністратор успішно створений");
        }
    }
    private static string GenerateSecurePassword(int length = 16)
    {
        const string uppercaseChars = "ABCDEFGHJKLMNPQRSTUVWXYZ";
        const string lowercaseChars = "abcdefghijkmnopqrstuvwxyz";
        const string digitChars = "23456789";
        const string specialChars = "!@#$%^&*()";

        var random = new Random();
        var password = new char[length];

        // Гарантуємо наявність символів кожного типу
        password[0] = uppercaseChars[random.Next(uppercaseChars.Length)];
        password[1] = lowercaseChars[random.Next(lowercaseChars.Length)];
        password[2] = digitChars[random.Next(digitChars.Length)];
        password[3] = specialChars[random.Next(specialChars.Length)];

        // Заповнюємо іншу частину паролю випадковими символами
        var allChars = uppercaseChars + lowercaseChars + digitChars + specialChars;
        for (int i = 4; i < length; i++)
        {
            password[i] = allChars[random.Next(allChars.Length)];
        }

        // Перемішуємо символи для безпеки
        for (int i = 0; i < length; i++)
        {
            int swapIndex = random.Next(length);
            (password[i], password[swapIndex]) = (password[swapIndex], password[i]);
        }

        return new string(password);
    }
}
