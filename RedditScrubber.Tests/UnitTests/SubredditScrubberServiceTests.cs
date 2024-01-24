using System.Threading.RateLimiting;
using Microsoft.Extensions.Logging.Abstractions;
using Microsoft.Extensions.Options;
using RedditScrubber.Core.Contracts;
using RedditScrubber.Core.Models;
using RedditScrubber.Core.Options;
using RedditScrubber.Core.Services;

namespace RedditScrubber.Tests.UnitTests;

public class SubredditScrubberServiceTests
{
    private readonly IRateLimiter _rateLimiter;
    private readonly IRedditApi _redditApi;
    private readonly IRedditRepository _repository;
    private readonly IOptions<RedditScrubberOptions> _options;
    private readonly Fixture _fixture;
    
    private readonly SubredditScrubberService _sut;

    public SubredditScrubberServiceTests()
    {
        _fixture = new Fixture();
        _rateLimiter = Substitute.For<IRateLimiter>();
        _redditApi = Substitute.For<IRedditApi>();
        _repository = Substitute.For<IRedditRepository>();
        _options = Substitute.For<IOptions<RedditScrubberOptions>>();
        _options.Value.Returns(new RedditScrubberOptions { RateLimitInMinutes = 1});
        _sut = new SubredditScrubberService("funny", _rateLimiter, _redditApi, _repository, _options,
            NullLogger<SubredditScrubberService>.Instance);
    }
    
    //Again, not much business logic
    [Fact]
    public async Task DoWorkAsync_Ok()
    {
        //arrange
        var threads = _fixture.Build<RedditThread>()
            .With(x => x.Subreddit, "funny")
            .CreateMany(10);
        _redditApi.GetLatestPostsForSubreddit(Arg.Any<string>(), Arg.Any<CancellationToken>())
            .ReturnsForAnyArgs(threads);
        _rateLimiter.GetStatistics().ReturnsForAnyArgs(new RateLimiterStatistics { CurrentAvailablePermits = 10 });
        
        var ctSource = new CancellationTokenSource(1000);
        var ct = ctSource.Token;
        
        //act
        await _sut.DoWorkAsync(ct);

        //assert
        await _redditApi.Received().GetLatestPostsForSubreddit("funny", ct);
        _repository.Received().Upsert(threads);
    }
}