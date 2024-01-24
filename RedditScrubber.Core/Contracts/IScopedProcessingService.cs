namespace RedditScrubber.Core.Contracts;

public interface IScopedProcessingService
{
    Task DoWorkAsync(CancellationToken cancellationToken);
}