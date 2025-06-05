using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace Application.Common.Abstractions
{
    /// <summary>
    /// Интерфейс для валидаторов запросов
    /// </summary>
    /// <typeparam name="T">Тип валидируемого объекта</typeparam>
    public interface IValidator<in T>
    {
        /// <summary>
        /// Асинхронно выполняет валидацию объекта
        /// </summary>
        /// <param name="instance">Объект для валидации</param>
        /// <param name="cancellationToken">Токен отмены операции</param>
        /// <returns>Результат валидации</returns>
        Task<ValidationResult> ValidateAsync(T instance, CancellationToken cancellationToken = default);
    }

    /// <summary>
    /// Результат валидации
    /// </summary>
    public class ValidationResult
    {
        /// <summary>
        /// Ошибки валидации
        /// </summary>
        public IReadOnlyList<ValidationError> Errors { get; }

        /// <summary>
        /// Успешно ли прошла валидация
        /// </summary>
        public bool IsValid => Errors == null || Errors.Count == 0;

        /// <summary>
        /// Создает новый экземпляр результата валидации
        /// </summary>
        /// <param name="errors">Список ошибок валидации</param>
        public ValidationResult(IReadOnlyList<ValidationError> errors = null)
        {
            Errors = errors ?? new List<ValidationError>();
        }

        /// <summary>
        /// Создает успешный результат валидации
        /// </summary>
        public static ValidationResult Success() => new();

        /// <summary>
        /// Создает результат валидации с ошибкой
        /// </summary>
        public static ValidationResult Error(string propertyName, string errorMessage) =>
            new(new[] { new ValidationError(propertyName, errorMessage) });
    }

    /// <summary>
    /// Ошибка валидации
    /// </summary>
    public class ValidationError
    {
        /// <summary>
        /// Имя свойства, на котором произошла ошибка
        /// </summary>
        public string PropertyName { get; }

        /// <summary>
        /// Сообщение об ошибке
        /// </summary>
        public string ErrorMessage { get; }

        /// <summary>
        /// Создает новый экземпляр ошибки валидации
        /// </summary>
        /// <param name="propertyName">Имя свойства</param>
        /// <param name="errorMessage">Сообщение об ошибке</param>
        public ValidationError(string propertyName, string errorMessage)
        {
            PropertyName = propertyName;
            ErrorMessage = errorMessage;
        }
    }
}
