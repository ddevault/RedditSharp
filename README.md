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

## Development

This gets improved every time I need it to have new features. I use it to power my various Reddit projects,
like the [/r/pokemon flair bot](https://github.com/SirCmpwn/PokemonFlairBot), or
[srutils](https://github.com/SirCmpwn/srutils), or various other projects. Whenever I need a feature it doesn't
have, I add it.

So! If you need a feature that it doesn't have, feel free to let me know. If you find bugs, also give me a
ring. Do this by making a [GitHub issue](https://github.com/SirCmpwn/RedditSharp/issues) or 
[emailing me](mailto:sir@cmpwn.com). You can also submit pull requests, which I'll happily accept if they're
of good quality.
