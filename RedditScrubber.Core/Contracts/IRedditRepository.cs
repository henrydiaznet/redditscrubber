using RedditScrubber.Core.Models;

namespace RedditScrubber.Core.Contracts;

public interface IRedditRepository
{
    int Upsert(IEnumerable<RedditThread> threads);
    IEnumerable<UserTop> GetTopUsers(string subreddit, CancellationToken cancellationToken);
    IEnumerable<RedditThread> GetTopThreads(string subreddit, CancellationToken cancellationToken);

}