# RedditSharp

A work-in-progress implementation of the [Reddit](http://reddit.com) API.

    var reddit = new Reddit();
    var user = reddit.Login("username", "password");
    Console.WriteLine("You have " + user.LinkKarma + " link karma!");
    var subreddit = reddit.GetSubreddit("/r/test");
    foreach (var post in subreddit.GetPosts())
        Console.WriteLine(post.Title);