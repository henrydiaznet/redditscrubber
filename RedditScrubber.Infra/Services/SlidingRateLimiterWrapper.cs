using System.Threading.RateLimiting;
using RedditScrubber.Core.Contracts;
using RedditScrubber.Core.Services;

namespace RedditScrubber.Infra.Services;

public class SlidingRateLimiterWrapper: IRateLimiter
{
    private readonly SlidingWindowRateLimiter _rateLimiter;

    public SlidingRateLimiterWrapper(SlidingWindowRateLimiter rateLimiter)
    {
        _rateLimiter = rateLimiter;
    }
    
    public ValueTask<RateLimitLease> AcquireAsync(CancellationToken cancellationToken)
    {
        return _rateLimiter.AcquireAsync(cancellationToken: cancellationToken);
    }

    public RateLimiterStatistics GetStatistics()
    {
        return _rateLimiter.GetStatistics();
    }
}