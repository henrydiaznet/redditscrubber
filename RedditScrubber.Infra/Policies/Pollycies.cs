using System.Net;
using Polly;
using Polly.Extensions.Http;

namespace RedditScrubber.Infra.Policies;

public static class Pollycies
{
    public static IAsyncPolicy<HttpResponseMessage> TransientRetry()
    {
        return HttpPolicyExtensions
            .HandleTransientHttpError()
            //this shouldn't really come into play, considering the rate limit being imposed on client side:
            .OrResult(message => message.StatusCode == HttpStatusCode.TooManyRequests) 
            .WaitAndRetryAsync(5, retryAttempt => TimeSpan.FromSeconds(Math.Pow(2,
                retryAttempt)));
    }
}