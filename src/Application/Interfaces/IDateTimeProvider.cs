using System;

namespace Application.Interfaces
{
    /// <summary>
    /// Сервис для получения текущей даты и времени
    /// </summary>
    /// <remarks>
    /// Этот интерфейс предоставляет абстракцию для получения текущей даты и времени,
    /// что позволяет легко заменять реализацию при тестировании и при необходимости
    /// использовать различные источники времени.
    /// </remarks>
    public interface IDateTimeProvider
    {
        /// <summary>
        /// Получает текущую дату и время в UTC
        /// </summary>
        DateTime UtcNow { get; }

        /// <summary>
        /// Получает текущую дату в UTC
        /// </summary>
        DateTime UtcToday { get; }

        /// <summary>
        /// Получает текущую дату и время для указанного часового пояса
        /// </summary>
        /// <param name="timeZoneId">Идентификатор часового пояса</param>
        /// <returns>Текущая дата и время в указанном часовом поясе</returns>
        DateTime GetCurrentTime(string timeZoneId);

        /// <summary>
        /// Преобразует UTC время в локальное время для указанного часового пояса
        /// </summary>
        /// <param name="utcTime">Время в UTC</param>
        /// <param name="timeZoneId">Идентификатор часового пояса</param>
        /// <returns>Локальное время для указанного часового пояса</returns>
        DateTime ConvertToLocalTime(DateTime utcTime, string timeZoneId);

        /// <summary>
        /// Получает смещение часового пояса относительно UTC
        /// </summary>
        /// <param name="timeZoneId">Идентификатор часового пояса</param>
        /// <returns>Смещение часового пояса</returns>
        TimeSpan GetTimeZoneOffset(string timeZoneId);
    }
}
