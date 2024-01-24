using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using RedditScrubber.Core.Contracts;
using RedditScrubber.Core.Options;

namespace RedditScrubber.Core.Services;

public class SubredditScrubberService : IScopedProcessingService
{
    private readonly IRateLimiter _rateLimiter;
    private readonly IRedditApi _redditApi;
    private readonly IRedditRepository _repository;
    private readonly ILogger<SubredditScrubberService> _logger;
    private readonly int _rateWindowInMilliseconds;
    private readonly string _subreddit;

    public SubredditScrubberService(string subreddit, 
        IRateLimiter rateLimiter, 
        IRedditApi redditApi,
        IRedditRepository repository,
        IOptions<RedditScrubberOptions> options,
        ILogger<SubredditScrubberService> logger)
    {
        _subreddit = subreddit;
        _rateLimiter = rateLimiter;
        _redditApi = redditApi;
        _repository = repository;
        _logger = logger;
        _rateWindowInMilliseconds = options.Value.RateLimitInMinutes * 60 * 1000;
    }

    public async Task DoWorkAsync(CancellationToken cancellationToken)
    {
        while (!cancellationToken.IsCancellationRequested)
        {
            try
            {
                //ratelimiter makes sure we don't make too many requests
                //alternatively, could have implemented delegating handler, which gets the rate limits directly from headers, added by Reddit API
                //but this solution can work with any API
                using var lease = await _rateLimiter.AcquireAsync(cancellationToken: cancellationToken);
                var threads = await _redditApi.GetLatestPostsForSubreddit(_subreddit, cancellationToken);
                _repository.Upsert(threads);
                var statistics = _rateLimiter.GetStatistics();
                //making sure we don't exhaust the permits
                await Task.Delay(_rateWindowInMilliseconds /(int) statistics.CurrentAvailablePermits, cancellationToken);
            }
            catch (Exception e)
            {
                _logger.LogError(e, "Error occurred while scrubbing {Subbreddit}: {Message}", _subreddit, e.Message);
            }
        }
    }
}