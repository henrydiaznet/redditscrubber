using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using RedditScrubber.Core.Contracts;

namespace RedditScrubber.Infra.Services;

//see https://learn.microsoft.com/en-us/dotnet/core/extensions/scoped-service
public sealed class ScopedBackgroundService : IHostedService, IDisposable
{
    private readonly string _subreddit;
    private readonly IServiceScopeFactory _serviceScopeFactory;
    private readonly ILogger<ScopedBackgroundService> _logger;
    private Task _executingTask = null!;
    private readonly CancellationTokenSource _stoppingCts = new();

    public ScopedBackgroundService(string subreddit, IServiceScopeFactory serviceScopeFactory, ILogger<ScopedBackgroundService> logger)
    {
        _subreddit = subreddit;
        _serviceScopeFactory = serviceScopeFactory;
        _logger = logger;
    }

    private async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        using var scope = _serviceScopeFactory.CreateScope();
        var factory = scope.ServiceProvider.GetRequiredService<IScrubberFactory>();
        var scrubber = factory.Create(_subreddit);
        _logger.LogInformation("Started scrubbing subreddit: {Subreddit}", _subreddit);
        await scrubber.DoWorkAsync(stoppingToken);
    }

    public Task StartAsync(CancellationToken cancellationToken)
    {
        _executingTask = ExecuteAsync(_stoppingCts.Token);
        if (_executingTask.IsCompleted)
        {
            return _executingTask;
        }
        return Task.CompletedTask;
    }

    public async Task StopAsync(CancellationToken cancellationToken)
    {
        if (_executingTask == null)
        {
            return;
        }

        try
        {
            _stoppingCts.Cancel();
        }
        finally
        {
            await Task.WhenAny(_executingTask, Task.Delay(Timeout.Infinite, cancellationToken));
        }
    }

    public void Dispose()
    {
        _stoppingCts.Cancel();
    }
}
