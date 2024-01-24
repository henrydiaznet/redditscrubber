using Microsoft.AspNetCore.Mvc;
using RedditScrubber.Core.Contracts;

namespace RedditScrubber.Api.Controllers;

[ApiController]
[Route("[controller]")]
public class RedditScrubberController : ControllerBase
{
    private readonly IRedditRepository _repository;

    public RedditScrubberController(IRedditRepository repository)
    {
        _repository = repository;
    }

    [HttpGet("api/r/{subreddit}/topthreads")]
    public IActionResult GetThreads(string subreddit, CancellationToken cancellationToken)
    {
        var result = _repository.GetTopThreads(subreddit, cancellationToken);
        return Ok(result);
    }
    
    [HttpGet("api/r/{subreddit}/topusers")]
    public IActionResult GetUsers(string subreddit, CancellationToken cancellationToken)
    {
        var result = _repository.GetTopUsers(subreddit, cancellationToken);
        return Ok(result);
    }
}