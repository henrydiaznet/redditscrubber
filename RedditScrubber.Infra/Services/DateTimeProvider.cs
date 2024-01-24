using RedditScrubber.Core.Contracts;

namespace RedditScrubber.Infra.Services;

public class DateTimeProvider: IClock
{
    public DateTime Now => DateTime.UtcNow;
}