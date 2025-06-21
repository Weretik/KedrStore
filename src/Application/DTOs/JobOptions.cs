namespace Application.DTOs;

public class JobOptions
{
    public string Queue { get; set; }
    public int Priority { get; set; } = 0;
    public int RetryCount { get; set; } = 3;
    public TimeSpan RetryDelay { get; set; } = TimeSpan.FromMinutes(5);
}
