using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace RedditSharp
{
    public class SubredditImage
    {
        private const string DeleteImageUrl = "http://www.reddit.com/api/delete_sr_img";

        private Reddit Reddit { get; set; }

        public SubredditImage(Reddit reddit, SubredditStyle subredditStyle,
            string cssLink, string name)
        {
            Reddit = reddit;
            SubredditStyle = subredditStyle;
            Name = name;
            CssLink = cssLink;
        }

        public SubredditImage(Reddit reddit, SubredditStyle subreddit,
            string cssLink, string name, string url)
            : this(reddit, subreddit, cssLink, name)
        {
            Url = url;
        }

        public string CssLink { get; set; }
        public string Name { get; set; }
        public string Url { get; set; }
        public SubredditStyle SubredditStyle { get; set; }

        public void Delete()
        {
            var request = Reddit.CreatePost(DeleteImageUrl);
            var stream = request.GetRequestStream();
            Reddit.WritePostBody(stream, new
            {
                img_name = Name,
                uh = Reddit.User.Modhash
            });
            SubredditStyle.Images.Remove(this);
        }
    }
}
