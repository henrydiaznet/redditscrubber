using System.Net.Http.Headers;
using RedditScrubber.Core.Contracts;

namespace RedditScrubber.Infra.Handlers;

public class AuthenticationDelegatingHandler: DelegatingHandler
{
    private readonly ITokenProvider _tokenProvider;

    public AuthenticationDelegatingHandler(ITokenProvider tokenProvider)
    {
        _tokenProvider = tokenProvider;
    }

    protected override async Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken)
    {
        request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", await _tokenProvider.GetToken(cancellationToken));
        return await base.SendAsync(request, cancellationToken);
    }
}