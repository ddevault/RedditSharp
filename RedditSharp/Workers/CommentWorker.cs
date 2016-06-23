using Newtonsoft.Json.Linq;
using RedditSharp.Models;
using System;
using System.Net;
using System.Threading.Tasks;

namespace RedditSharp.Workers
{
    public class CommentWorker
    {
        private const string GetCommentUrl = "/r/{0}/comments/{1}/foo/{2}";
        private const string GetPostUrl = "{0}.json";

        private Reddit reddit;

        public CommentWorker(Reddit reddit)
        {
            this.reddit = reddit;
        }

        public Comment GetComment(string subreddit, string name, string linkName)
        {
            try
            {
                if (linkName.StartsWith("t3_"))
                    linkName = linkName.Substring(3);
                if (name.StartsWith("t1_"))
                    name = name.Substring(3);

                var url = string.Format(GetCommentUrl, subreddit, linkName, name);
                return GetComment(new Uri(url));
            }
            catch (WebException)
            {
                return null;
            }
        }

        public Comment GetComment(Uri uri)
        {
            var url = string.Format(GetPostUrl, uri.AbsoluteUri);
            var request = reddit.WebAgent.CreateGet(url);
            var response = request.GetResponse();
            var data = reddit.WebAgent.GetResponseString(response.GetResponseStream());
            var json = JToken.Parse(data);

            var sender = new Post().Init(reddit, json[0]["data"]["children"][0], reddit.WebAgent);
            return new Comment().Init(reddit, json[1]["data"]["children"][0], reddit.WebAgent, sender);
        }

    }
}
