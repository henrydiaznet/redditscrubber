using System.Threading.RateLimiting;

namespace RedditScrubber.Core.Contracts;

public interface IRateLimiter
{
    ValueTask<RateLimitLease> AcquireAsync(CancellationToken cancellationToken);
    RateLimiterStatistics GetStatistics();
}