using System.ComponentModel.DataAnnotations;

namespace RedditScrubber.Core.Options;

public class RedditApiOptions
{
    public static string Section = "RedditApi";

    [Required]
    public string ClientId { get; set; }
    [Required]
    public string ClientSecret { get; set; }
}