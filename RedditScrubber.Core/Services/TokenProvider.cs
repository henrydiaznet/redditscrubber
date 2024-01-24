using System.Net.Http.Headers;
using System.Net.Http.Json;
using System.Text;
using Microsoft.Extensions.Options;
using RedditScrubber.Core.Contracts;
using RedditScrubber.Core.Models;
using RedditScrubber.Core.Options;

namespace RedditScrubber.Core.Services;

public class TokenProvider : ITokenProvider
{
    private readonly HttpClient _client;
    private readonly RedditApiOptions _credentialInfo;
    private readonly IClock _clock;
    private TokenHolder _currentToken = new();

    public TokenProvider(HttpClient client, IClock clock, IOptions<RedditApiOptions> options)
    {
        _client = client;
        _credentialInfo = options.Value;
        _clock = clock;
    }

    public async Task<string> GetToken(CancellationToken cancellationToken)
    {
        if (_currentToken.ExpirationDate > _clock.Now)
        {
            return _currentToken.Token;
        }

        var tokenReceived = await RequestToken(cancellationToken);
        _currentToken = new TokenHolder
        {
            Token = tokenReceived.Token,
            ExpirationDate = _clock.Now
                .AddSeconds(tokenReceived.ExpirationInSeconds)
                .AddSeconds(-10) //better to have some buffer in case we try to use token right before expiration
        };
        
        return _currentToken.Token;
    }

    private async Task<TokenResponse> RequestToken(CancellationToken cancellationToken)
    {
        var authenticationString = $"{_credentialInfo.ClientId}:{_credentialInfo.ClientSecret}";
        var base64EncodedAuthenticationString = Convert.ToBase64String(Encoding.UTF8.GetBytes(authenticationString));
        _client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Basic", base64EncodedAuthenticationString);
        var response = await _client.PostAsync("api/v1/access_token", 
            new FormUrlEncodedContent(new []
            {
                new KeyValuePair<string, string>("grant_type", "client_credentials"),

            }),
            cancellationToken);
        
        response.EnsureSuccessStatusCode();
        return await response.Content.ReadFromJsonAsync<TokenResponse>(cancellationToken: cancellationToken);
    }

    private class TokenHolder
    {
        public string Token { get; set; }
        public DateTime ExpirationDate { get; set; }
    }
}