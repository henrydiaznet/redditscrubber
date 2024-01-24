namespace RedditScrubber.Core.Models;

public class RedditThread
{
    public string Id { get; set; }
    public string UserId { get; set; }
    public string UserName { get; set; }
    public int Upvotes { get; set; }
    public string Title { get; set; }
    public string Subreddit { get; set; }    
    public DateTimeOffset CreatedAtUtc { get; set; }

    public RedditThread()
    { }

    public RedditThread(Data1 input)
    {
        Id = input.id;
        UserId = input.author_fullname;
        UserName = input.author;        
        Upvotes = input.ups;
        Title = input.title;
        Subreddit = input.subreddit;
        CreatedAtUtc = DateTimeOffset.FromUnixTimeSeconds(Convert.ToInt64(input.created_utc));
    }
}