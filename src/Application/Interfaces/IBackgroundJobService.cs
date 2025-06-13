namespace Application.Interfaces
{
    /// <summary>
    /// Параметры для фоновых задач
    /// </summary>
    public class JobOptions
    {
        /// <summary>
        /// Идентификатор очереди или группы задач
        /// </summary>
        public string Queue { get; set; }

        /// <summary>
        /// Приоритет задачи
        /// </summary>
        public int Priority { get; set; } = 0;

        /// <summary>
        /// Максимальное количество попыток выполнения
        /// </summary>
        public int RetryCount { get; set; } = 3;

        /// <summary>
        /// Интервал между повторами в случае ошибки
        /// </summary>
        public TimeSpan RetryDelay { get; set; } = TimeSpan.FromMinutes(5);
    }

    /// <summary>
    /// Сервис для работы с фоновыми задачами
    /// </summary>
    /// <remarks>
    /// Этот интерфейс предоставляет абстракцию для управления фоновыми задачами,
    /// позволяя использовать различные реализации (Hangfire, Quartz.NET, Background Service и т.д.).
    /// </remarks>
    public interface IBackgroundJobService
    {
        /// <summary>
        /// Ставит задачу в очередь на немедленное выполнение
        /// </summary>
        /// <typeparam name="T">Тип обработчика задачи</typeparam>
        /// <param name="args">Аргументы для обработчика</param>
        /// <param name="options">Параметры задачи</param>
        /// <returns>Идентификатор созданной задачи</returns>
        string Enqueue<T>(object args = null, JobOptions options = null) where T : IJobHandler;

        /// <summary>
        /// Ставит задачу в очередь на отложенное выполнение
        /// </summary>
        /// <typeparam name="T">Тип обработчика задачи</typeparam>
        /// <param name="delay">Задержка перед выполнением</param>
        /// <param name="args">Аргументы для обработчика</param>
        /// <param name="options">Параметры задачи</param>
        /// <returns>Идентификатор созданной задачи</returns>
        string Schedule<T>(TimeSpan delay, object args = null, JobOptions options = null) where T : IJobHandler;

        /// <summary>
        /// Ставит задачу в очередь на выполнение в указанное время
        /// </summary>
        /// <typeparam name="T">Тип обработчика задачи</typeparam>
        /// <param name="enqueueAt">Дата и время выполнения</param>
        /// <param name="args">Аргументы для обработчика</param>
        /// <param name="options">Параметры задачи</param>
        /// <returns>Идентификатор созданной задачи</returns>
        string Schedule<T>(DateTime enqueueAt, object args = null, JobOptions options = null) where T : IJobHandler;

        /// <summary>
        /// Создает периодическую задачу по указанному расписанию
        /// </summary>
        /// <typeparam name="T">Тип обработчика задачи</typeparam>
        /// <param name="cronExpression">Выражение CRON для расписания</param>
        /// <param name="args">Аргументы для обработчика</param>
        /// <param name="options">Параметры задачи</param>
        /// <returns>Идентификатор созданной задачи</returns>
        string RecurringJob<T>(string cronExpression, object args = null, JobOptions options = null) where T : IJobHandler;

        /// <summary>
        /// Удаляет задачу из очереди
        /// </summary>
        /// <param name="jobId">Идентификатор задачи</param>
        /// <returns>true, если задача успешно удалена, иначе false</returns>
        bool Delete(string jobId);

        /// <summary>
        /// Проверяет существование задачи
        /// </summary>
        /// <param name="jobId">Идентификатор задачи</param>
        /// <returns>true, если задача существует, иначе false</returns>
        bool Exists(string jobId);

        /// <summary>
        /// Выполняет задачу немедленно
        /// </summary>
        /// <param name="jobId">Идентификатор задачи</param>
        /// <returns>true, если задача успешно запущена, иначе false</returns>
        bool Trigger(string jobId);
    }

    /// <summary>
    /// Интерфейс для обработчиков фоновых задач
    /// </summary>
    public interface IJobHandler
    {
        /// <summary>
        /// Выполняет задачу
        /// </summary>
        /// <param name="args">Аргументы задачи</param>
        /// <param name="cancellationToken">Токен отмены</param>
        /// <returns>Задача, представляющая асинхронную операцию</returns>
        Task Execute(object args, CancellationToken cancellationToken = default);
    }
}
