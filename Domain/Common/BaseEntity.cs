namespace Domain.Common
{
    public class BaseEntity<T>
    {
        public T Id { get; protected set; } = default!;
        public DateTime CreatedAt { get; protected set; } = DateTime.UtcNow;
        public DateTime UpdatedAt { get; protected set; } = DateTime.UtcNow;
        public bool IsDeleted { get; protected set; } = false;
        
        public void MarkAsDeleted()
        {
            IsDeleted = true;
            UpdatedAt = DateTime.UtcNow;
        }
        public void MarkAsUpdated() => UpdatedAt = DateTime.UtcNow;
        
    }
}
