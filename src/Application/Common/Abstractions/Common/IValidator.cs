namespace Application.Common.Abstractions.Common
{
    /// <summary>
    /// Интерфейс для валидаторов
    /// </summary>
    /// <typeparam name="T">Тип валидируемого объекта</typeparam>
    public interface IValidator<in T>
    {
        /// <summary>
        /// Выполняет валидацию объекта
        /// </summary>
        /// <param name="instance">Валидируемый объект</param>
        /// <param name="cancellationToken">Токен отмены</param>
        /// <returns>Результат валидации</returns>
        Task<ValidationResult> ValidateAsync(T instance, CancellationToken cancellationToken = default);
    }

    /// <summary>
    /// Результат валидации
    /// </summary>
    public class ValidationResult
    {
        /// <summary>
        /// Список ошибок валидации
        /// </summary>
        public IReadOnlyList<ValidationError> Errors { get; }

        /// <summary>
        /// Признак успешной валидации
        /// </summary>
        public bool IsValid => Errors == null || Errors.Count == 0;

        public ValidationResult(IReadOnlyList<ValidationError> errors = null)
        {
            Errors = errors ?? new List<ValidationError>();
        }
    }

    /// <summary>
    /// Ошибка валидации
    /// </summary>
    public class ValidationError
    {
        /// <summary>
        /// Имя свойства, в котором произошла ошибка
        /// </summary>
        public string PropertyName { get; }

        /// <summary>
        /// Сообщение об ошибке
        /// </summary>
        public string ErrorMessage { get; }

        public ValidationError(string propertyName, string errorMessage)
        {
            PropertyName = propertyName;
            ErrorMessage = errorMessage;
        }
    }
}
