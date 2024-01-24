using RedditScrubber.Core.Models;

namespace RedditScrubber.Core.Contracts;

public interface IRedditApi
{
    Task<IEnumerable<RedditThread>> GetLatestPostsForSubreddit(string subreddit, CancellationToken cancellationToken);
}