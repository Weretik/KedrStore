using Domain.Events;
using Domain.Identity.Entities;
using Domain.Identity.ValueObjects;
using Domain.Events;
using Domain.Identity.Entities;
using Domain.Identity.ValueObjects;

namespace Domain.Identity.Events
{
    /// <summary>
    /// Событие, представляющее регистрацию нового пользователя в системе
    /// </summary>
    public class UserRegisteredEvent : BaseDomainEvent
    {
        public AppUserId UserId { get; }
        public string FullName { get; }
        public string Email { get; }

        public UserRegisteredEvent(AppUser user)
        {
            UserId = user.Id;
            FullName = user.FullName;
            Email = user.Email;

        }
    }
}
