namespace Application.DTOs;

public sealed class CacheOptions
{
    public TimeSpan? ExpirationTime { get; set; }
    public string[] Tags { get; set; } = [];
    public bool SlidingExpiration { get; set; } = false;
}
