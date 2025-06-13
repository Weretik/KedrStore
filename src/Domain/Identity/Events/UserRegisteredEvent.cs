namespace Domain.Identity.Events
{
    /// <summary>
    /// Событие, представляющее регистрацию нового пользователя в системе
    /// </summary>
    public class UserRegisteredEvent(int userId, string fullName, string? email=null) : BaseDomainEvent
    {
        public int UserId { get; } = userId;
        public string FullName { get; } = fullName;
        public string? Email { get; } = email;
    }
}
