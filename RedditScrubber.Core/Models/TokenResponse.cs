using System.Text.Json.Serialization;

namespace RedditScrubber.Core.Models;

public class TokenResponse
{
    [JsonPropertyName("access_token")]
    public string Token { get; set; }
    [JsonPropertyName("expires_in")]
    public int ExpirationInSeconds { get; set; }
}