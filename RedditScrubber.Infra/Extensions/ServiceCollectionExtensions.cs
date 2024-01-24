using System.Net.Http.Headers;
using System.Threading.RateLimiting;
using LiteDB;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using RedditScrubber.Core.Contracts;
using RedditScrubber.Core.Options;
using RedditScrubber.Core.Services;
using RedditScrubber.Infra.Handlers;
using RedditScrubber.Infra.Policies;
using RedditScrubber.Infra.Services;

namespace RedditScrubber.Infra.Extensions;

public static class ServiceCollectionExtensions
{
    public static IServiceCollection AddServices(this IServiceCollection services, IConfiguration configuration)
    {
        //if we have multiple httpclients for scrubbing, they will share this IRateLimiter
        services.AddSingleton<IRateLimiter>(provider =>
        {
            var options = provider.GetRequiredService<IOptions<RedditScrubberOptions>>().Value;
            var slidingWindowRateLimiter = new SlidingWindowRateLimiter(new SlidingWindowRateLimiterOptions
            {
                PermitLimit = options.RateLimitPerWindow,
                SegmentsPerWindow = 20,
                QueueProcessingOrder = QueueProcessingOrder.OldestFirst,
                Window = TimeSpan.FromMinutes(options.RateLimitInMinutes),
                AutoReplenishment = true
            });
            return new SlidingRateLimiterWrapper(slidingWindowRateLimiter);
        });

        services.AddSingleton<IClock, DateTimeProvider>();
        services.AddTransient<AuthenticationDelegatingHandler>();
        services.AddScoped<IRedditRepository, Repository>();
        services.AddScoped<IScrubberFactory, ScrubberFactory>();
        
        //rn all the scrubber instances share the database, but it would be fairly trivial to configure DB instance per subreddit
        services.AddScoped<ILiteDatabase>(_ => new LiteDatabase(configuration.GetConnectionString("RedditDb")));
        
        //just add config values if you want to scrub more subreddits
        var subreddits = configuration.GetSection("RedditScrubber:Subreddits").Get<List<string>>();
        if (subreddits is not null && subreddits.Any())
        {
            foreach (var subreddit in subreddits)
            {
                //services.AddHostedService adds strongly typed Singleton (so it would only add one), this is a workaround
                services.AddSingleton<IHostedService, ScopedBackgroundService>(provider => 
                    new ScopedBackgroundService(subreddit, 
                        provider.GetRequiredService<IServiceScopeFactory>(), 
                        provider.GetRequiredService<ILogger<ScopedBackgroundService>>()));
            }
        }

        return services;
    }

    public static IServiceCollection AddHttpClients(this IServiceCollection services, IConfiguration configuration)
    {
        services.AddHttpClient<ITokenProvider, TokenProvider>(client =>
            {
                client.DefaultRequestHeaders.UserAgent.Add(new ProductInfoHeaderValue("SubredditSubscriber", "0.1"));
                client.DefaultRequestHeaders.UserAgent.Add(new ProductInfoHeaderValue("(+by /u/SolutionBest9052)"));
                client.BaseAddress = new Uri("https://www.reddit.com/");
            })
            .AddPolicyHandler(Pollycies.TransientRetry());

        services.AddHttpClient<IRedditApi, RedditApi>(client =>
            {
                client.DefaultRequestHeaders.UserAgent.Add(new ProductInfoHeaderValue("SubredditSubscriber", "0.1"));
                client.DefaultRequestHeaders.UserAgent.Add(new ProductInfoHeaderValue("(+by /u/SolutionBest9052)"));
                client.BaseAddress = new Uri("https://oauth.reddit.com/");
            })
            .AddHttpMessageHandler<AuthenticationDelegatingHandler>()
            .AddPolicyHandler(Pollycies.TransientRetry());

        return services;
    }
}