namespace RedditSharp
{
    public class SubredditImage
    {
        private const string DeleteImageUrl = "/api/delete_sr_img";

        private Reddit Reddit { get; set; }
        private IWebAgent WebAgent { get; set; }

        public SubredditImage(Reddit reddit, SubredditStyle subredditStyle,
            string cssLink, string name, IWebAgent webAgent)
        {
            Reddit = reddit;
            WebAgent = webAgent;
            SubredditStyle = subredditStyle;
            Name = name;
            CssLink = cssLink;
        }

        public SubredditImage(Reddit reddit, SubredditStyle subreddit,
            string cssLink, string name, string url, IWebAgent webAgent)
            : this(reddit, subreddit, cssLink, name, webAgent)
        {
            Url = url;
            // Handle legacy image urls
            // http://thumbs.reddit.com/FULLNAME_NUMBER.png
            int discarded;
            if (int.TryParse(url, out discarded))
                Url = string.Format("http://thumbs.reddit.com/{0}_{1}.png", subreddit.Subreddit.FullName, url);
        }

        public string CssLink { get; set; }
        public string Name { get; set; }
        public string Url { get; set; }
        public SubredditStyle SubredditStyle { get; set; }

        public void Delete()
        {
            var request = WebAgent.CreatePost(DeleteImageUrl);
            var stream = request.GetRequestStream();
            WebAgent.WritePostBody(stream, new
            {
                img_name = Name,
                uh = Reddit.User.Modhash,
                r = SubredditStyle.Subreddit.Name
            });
            stream.Close();
            var response = request.GetResponse();
            var data = WebAgent.GetResponseString(response.GetResponseStream());
            SubredditStyle.Images.Remove(this);
        }
    }
}
