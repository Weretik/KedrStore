using Domain.Common;

namespace Domain.Identity.ValueObjects
{
    public class AppUserId : EntityId
    {
        public AppUserId(int value) : base(value) { }

    }
}
