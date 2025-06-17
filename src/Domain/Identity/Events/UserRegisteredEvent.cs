namespace Domain.Identity.Events
{
    public class UserRegisteredEvent(int userId, string fullName, string? email=null) : BaseDomainEvent
    {
        public int UserId { get; } = userId;
        public string FullName { get; } = fullName;
        public string? Email { get; } = email;
    }
}
