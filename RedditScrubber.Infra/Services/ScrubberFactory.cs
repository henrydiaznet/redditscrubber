using System.Threading.RateLimiting;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using RedditScrubber.Core.Contracts;
using RedditScrubber.Core.Options;
using RedditScrubber.Core.Services;

namespace RedditScrubber.Infra.Services;

public class ScrubberFactory: IScrubberFactory
{
    private readonly IRateLimiter _rateLimiter;
    private readonly IRedditApi _redditApi;
    private readonly IRedditRepository _repository;
    private readonly IOptions<RedditScrubberOptions> _options;
    private readonly ILoggerFactory _loggerFactory;

    public ScrubberFactory(IRateLimiter rateLimiter, IRedditApi redditApi, IRedditRepository repository, IOptions<RedditScrubberOptions> options, ILoggerFactory loggerFactory)
    {
        _rateLimiter = rateLimiter;
        _redditApi = redditApi;
        _repository = repository;
        _options = options;
        _loggerFactory = loggerFactory;
    }
    
    //there should be a way to do it in a cooler way, but i didn't want to spend more time on this
    //UPDATE in .NET8 they added keyed services
    public IScopedProcessingService Create(string subreddit)
    {
        return new SubredditScrubberService(subreddit,
            _rateLimiter,
            _redditApi,
            _repository,
            _options,
            _loggerFactory.CreateLogger<SubredditScrubberService>());
    }
}