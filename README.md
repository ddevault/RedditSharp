# RedditSharp

A partial implementation of the [Reddit](http://reddit.com) API. Includes support for many API endpoints, as well as
LINQ-style paging of results.

```csharp
var reddit = new Reddit();
var user = reddit.LogIn("username", "password");
var subreddit = reddit.GetSubreddit("/r/example");
subreddit.Subscribe();
var posts = subreddit.GetNew();
foreach (var post in posts.Take(25))
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

**Important note**: Make sure you use `.Take(int)` when working with pagable content. For example, don't do this:

```csharp
var all = reddit.RSlashAll();
foreach (var post in all)
{
    // ...
}
```

This will cause you to page through everything that has ever been posted on Reddit. Better:

```csharp
var all = reddit.RSlashAll();
foreach (var post in all.Take(25))
{
    // ...
}
```

Here's another example: you've made a bot to periodically check your subreddit's new page for things to automatically
remove:

```csharp
var subreddit = reddit.GetSubreddit("/r/myawesomesubreddit");
var newPosts = subreddit.GetNew();
var latest = newPosts.Skip(24).First();
while (true)
{
    // Gets all posts since the last post checked
    var toCheck = newPosts.TakeWhile(p => p != latest).ToArray();
    CheckPosts(toCheck);
    Thread.Sleep(60000);
}
```

## Development

RedditSharp is developed with the following workflow:

1. Nothing happens for weeks
2. Someone needs it to do something it doesn't already do
3. That person implements that something and submits a pull request
4. Repeat

If it doesn't have a feature that you want it to have, add it! The code isn't that scary.
