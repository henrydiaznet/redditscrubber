using System.Net.Http.Json;
using System.Text.Json;
using Microsoft.Extensions.Options;
using RedditScrubber.Core.Contracts;
using RedditScrubber.Core.Models;
using RedditScrubber.Core.Options;

namespace RedditScrubber.Core.Services;

public class RedditApi: IRedditApi
{
    private readonly HttpClient _client;
    private readonly RedditScrubberOptions _options;

    public RedditApi(HttpClient client, IOptions<RedditScrubberOptions> options)
    {
        _client = client;
        _options = options.Value;
    }
    
    public async Task<IEnumerable<RedditThread>> GetLatestPostsForSubreddit(string subreddit, CancellationToken cancellationToken)
    {
        var response = await _client.GetAsync($"/{subreddit}/new.json?limit={_options.LimitResults}", cancellationToken);
        response.EnsureSuccessStatusCode();

        var result = await response.Content.ReadFromJsonAsync<RootObject>(new JsonSerializerOptions(), cancellationToken);
        return result.data.children.Select(x => new RedditThread(x.data));
    }
}