namespace RedditScrubber.Core.Contracts;

public interface ITokenProvider
{
    Task<string> GetToken(CancellationToken cancellationToken);
}