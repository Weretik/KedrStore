namespace Application.Common.Events
{
    /// <summary>
    /// Базовый класс для всех доменных событий
    /// </summary>
    public abstract class DomainEvent : IDomainEvent
    {
        /// <summary>
        /// Уникальный идентификатор события
        /// </summary>
        public Guid Id { get; }

        /// <summary>
        /// Дата и время возникновения события
        /// </summary>
        public DateTime OccurredOn { get; }

        protected DomainEvent()
        {
            Id = Guid.NewGuid();
            OccurredOn = DateTime.UtcNow;
        }
    }
}
