namespace GameTimer.Abstractions;

public interface IClock
{
    /// <summary>
    /// 현재 UTC 시간
    /// </summary>
    DateTime UtcNow { get; }
}