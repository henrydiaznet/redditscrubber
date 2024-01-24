using System.Net.Http.Json;
using LiteDB;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.Extensions.DependencyInjection;
using RedditScrubber.Api;
using RedditScrubber.Core.Contracts;
using RedditScrubber.Core.Models;

namespace RedditScrubber.Tests.Api;

//this is mostly pointless as there is no business logic
public class RedditScrubberControllerTests: IClassFixture<CustomWebApplicationFactory<Program>>
{
    private readonly HttpClient _client;
    private readonly CustomWebApplicationFactory<Program> _factory;
    private readonly Fixture _fixture;

    public RedditScrubberControllerTests(
        CustomWebApplicationFactory<Program> factory)
    {
        _factory = factory;
        _client = factory.CreateClient(new WebApplicationFactoryClientOptions
        {
            AllowAutoRedirect = false
        });
        _fixture = new Fixture();
        PreSeedData();
    }
    
    [Fact]
    public async Task GetTopThreads()
    {
        // Arrange

        //Act
        var response = await _client.GetFromJsonAsync<IEnumerable<RedditThread>>($"RedditScrubber/api/r/funny/topthreads");

        // Assert
        response.Should().NotBeNull();
        response.Should().NotBeEmpty();
        response.Count().Should().Be(10);
    }
    
    [Fact]
    public async Task GetTopUsers()
    {
        // Arrange

        //Act
        var response = await _client.GetFromJsonAsync<IEnumerable<UserTop>>($"RedditScrubber/api/r/funny/topusers");

        // Assert
        response.Should().NotBeNull();
        response.Should().NotBeEmpty();
        var user1 = response.First();
        user1.Id.Should().Be("user1");
        user1.Posts.Should().Be(5);
    }
    
    private void PreSeedData()
    {
        var scope = _factory.Services.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<ILiteDatabase>();
        db.DropCollection(nameof(RedditThread));
        var repo = scope.ServiceProvider.GetRequiredService<IRedditRepository>();
        var seed = _fixture
            .Build<RedditThread>()
            .With(x => x.Subreddit, "funny")
            .CreateMany(5);
        var seed2 = _fixture
            .Build<RedditThread>()
            .With(x => x.Subreddit, "funny")
            .With(x => x.UserId, "user1")
            .With(x => x.UserName, "user1")
            .CreateMany(5);
        
        repo.Upsert(seed);
        repo.Upsert(seed2);
    }
}