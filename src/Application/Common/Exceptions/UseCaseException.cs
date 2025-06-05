namespace Application.Common.Exceptions
{
    /// <summary>
    /// Базовое исключение для сценариев использования приложения
    /// </summary>
    public class UseCaseException : Exception
    {
        /// <summary>
        /// Инициализирует новый экземпляр исключения сценария использования
        /// </summary>
        public UseCaseException() { }

        /// <summary>
        /// Инициализирует новый экземпляр исключения сценария использования с указанным сообщением
        /// </summary>
        /// <param name="message">Сообщение, описывающее ошибку</param>
        public UseCaseException(string message) : base(message) { }

        /// <summary>
        /// Инициализирует новый экземпляр исключения сценария использования с указанным сообщением
        /// и внутренним исключением
        /// </summary>
        /// <param name="message">Сообщение, описывающее ошибку</param>
        /// <param name="innerException">Исключение, вызвавшее текущее исключение</param>
        public UseCaseException(string message, Exception innerException) : base(message, innerException)
        {
        }
    }
}
