using RedditScrubber.Core.Options;
using RedditScrubber.Infra.Extensions;

var builder = WebApplication.CreateBuilder(args);
builder.Host.UseDefaultServiceProvider(options =>
{
    options.ValidateScopes = true;
    options.ValidateOnBuild = true;
});

// Add services to the container.

builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

builder.Services.AddServices(builder.Configuration);
builder.Services.AddHttpClients(builder.Configuration);

builder.Services.AddOptions<RedditScrubberOptions>()
    .Bind(builder.Configuration.GetSection(RedditScrubberOptions.Section))
    .ValidateDataAnnotations()
    .ValidateOnStart();

builder.Services.AddOptions<RedditApiOptions>()
    .Bind(builder.Configuration.GetSection(RedditApiOptions.Section))
    .ValidateDataAnnotations()
    .ValidateOnStart();

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();

app.UseAuthorization();

app.MapControllers();

app.Run();

//required for integration tests
namespace RedditScrubber.Api
{
    public partial class Program { }
}