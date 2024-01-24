using LiteDB;
using RedditScrubber.Core.Contracts;
using RedditScrubber.Core.Models;

namespace RedditScrubber.Infra.Services;

public class Repository : IRedditRepository
{
    //I know task said its not necessary, however litedb is somehow easier than implementing in memory collection
    private readonly ILiteCollection<RedditThread> _threads;

    public Repository(ILiteDatabase db)
    {
        _threads = db.GetCollection<RedditThread>();
        _threads.EnsureIndex(x => x.Subreddit);
    }

    public int Upsert(IEnumerable<RedditThread> threads)
    {
        return _threads.Upsert(threads);
    }

    public IEnumerable<UserTop> GetTopUsers(string subreddit, CancellationToken cancellationToken)
    {
        return _threads
            .Find(x => x.Subreddit == subreddit)
            .GroupBy(t => new
            {
                Id = t.UserId,
                Name = t.UserName
            })
            .Select(x => new UserTop
            {
                Id = x.Key.Id,
                Name = x.Key.Name,
                Upvotes = x.Sum(thread => thread.Upvotes),
                Posts = x.Count()
            })
            .OrderByDescending(x => x.Posts);
    }

    public IEnumerable<RedditThread> GetTopThreads(string subreddit, CancellationToken cancellationToken)
    {
        return _threads
            .Find(x => x.Subreddit == subreddit)
            .OrderByDescending(x => x.Upvotes);
    }
}