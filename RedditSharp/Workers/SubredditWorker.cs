using RedditSharp.Models;
using System.Threading.Tasks;

namespace RedditSharp.Workers
{
    public class SubredditWorker
    {
        private const string SubredditAboutUrl = "/r/{0}/about.json";

        private Reddit reddit;

        public SubredditWorker(Reddit reddit)
        {
            this.reddit = reddit;
        }

        public Subreddit GetSubreddit(string name)
        {
            if (name.StartsWith("r/"))
                name = name.Substring(2);
            if (name.StartsWith("/r/"))
                name = name.Substring(3);
            name = name.TrimEnd('/');
            return reddit.GetModel<Subreddit>(string.Format(SubredditAboutUrl, name));
        }

    }
}
