using System;
using System.Linq;
using System.Text.RegularExpressions;

namespace Application.Common.Validation
{
    /// <summary>
    /// Утилиты для валидации значений без выброса исключений
    /// </summary>
    public static class ValidationUtils
    {
        /// <summary>
        /// Проверяет, является ли строка действительным email адресом
        /// </summary>
        /// <returns>true, если строка является действительным email адресом, иначе false</returns>
        public static bool IsValidEmail(string email)
        {
            if (string.IsNullOrWhiteSpace(email))
                return false;

            try
            {
                return Regex.IsMatch(email,
                    @"^[^@\s]+@[^@\s]+\.[^@\s]+$",
                    RegexOptions.IgnoreCase);
            }
            catch (RegexMatchTimeoutException)
            {
                return false;
            }
        }

        /// <summary>
        /// Проверяет, является ли строка допустимым URL
        /// </summary>
        /// <returns>true, если строка является допустимым URL, иначе false</returns>
        public static bool IsValidUrl(string url)
        {
            return Uri.TryCreate(url, UriKind.Absolute, out _);
        }

        /// <summary>
        /// Проверяет, находится ли значение в допустимом диапазоне
        /// </summary>
        public static bool IsInRange<T>(T value, T min, T max) where T : IComparable<T>
        {
            return value.CompareTo(min) >= 0 && value.CompareTo(max) <= 0;
        }
    }
}
