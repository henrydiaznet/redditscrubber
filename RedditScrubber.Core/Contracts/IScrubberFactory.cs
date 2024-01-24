namespace RedditScrubber.Core.Contracts;

public interface IScrubberFactory
{
    IScopedProcessingService Create(string subreddit);
}