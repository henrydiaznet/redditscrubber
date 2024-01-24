namespace RedditScrubber.Core.Contracts;

public interface IClock
{
    DateTime Now { get; }
}