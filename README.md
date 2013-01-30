# RedditSharp

A partial implementation of the [Reddit](http://reddit.com) API.

```csharp
var reddit = new Reddit();
var user = reddit.LogIn("username", "password");
var subreddit = reddit.GetSubreddit("/r/example");
subreddit.Subscribe();
var posts = subreddit.GetNew();
foreach (var post in posts)
{
    if (post.Title == "What is my karma?")
    {
        // Note: This is an example. Bots are not permitted to cast votes automatically.
        post.Upvote();
        var comment = post.Comment(string.Format("You have {0} link karma!", post.Author.LinkKarma));
        comment.Distinguish(DistinguishType.Moderator);
    }
}
```

This gets improved every time I need it to have new features.
