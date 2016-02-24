# RedditSharp

**Hey, listen**! The Reddit admins are making some really crappy changes to the
API, and I need you to speak out against them. The changes
[are here](https://www.reddit.com/r/redditdev/comments/3xdf11/introducing_new_api_terms/)
and you should
[message the admins](https://www.reddit.com/message/compose?to=/r/reddit.com&subject=The+planned+API+changes+are+awful&message=Hey,+the+planned+API+changes+are+pretty+awful.+You%27re+asking+users+to+reveal+far+too+much+about+themselves+and+making+the+barrier+to+entry+much+higher+for+new+developers+who+want+to+do+cool+things+with+the+Reddit+API.+You+already+ask+users+to+put+contact+information+in+their+user+agent+string,+and+that+should+be+sufficient.)
if you disagree with them (you're welcome to change this message to something
more personal).

A partial implementation of the [Reddit](http://reddit.com) API. Includes support for many API endpoints, as well as
LINQ-style paging of results.

```csharp
var reddit = new Reddit();
var user = reddit.LogIn("username", "password");
var subreddit = reddit.GetSubreddit("/r/example");
subreddit.Subscribe();
foreach (var post in subreddit.New.Take(25))
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
var all = reddit.RSlashAll;
foreach (var post in all) // BAD
{
    // ...
}
```

This will cause you to page through everything that has ever been posted on Reddit. Better:

```csharp
var all = reddit.RSlashAll;
foreach (var post in all.Take(25))
{
    // ...
}
```

## Development

RedditSharp is developed with the following workflow:

1. Nothing happens for weeks/months/years
2. Someone needs it to do something it doesn't already do
3. That person implements that something and submits a pull request
4. Repeat

If it doesn't have a feature that you want it to have, add it! The code isn't that scary.
