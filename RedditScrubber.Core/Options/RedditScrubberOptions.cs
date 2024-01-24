using System.ComponentModel.DataAnnotations;

namespace RedditScrubber.Core.Options;

public class RedditScrubberOptions
{
    public static string Section = "RedditScrubber";

    [Required]
    public string[] Subreddits { get; set; }
    public int RateLimitPerWindow { get; set; } = 600; 
    public int RateLimitInMinutes { get; set; } = 6;
    public int LimitResults { get; set; } = 25;
}