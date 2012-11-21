using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Newtonsoft.Json.Linq;

namespace RedditSharp
{
    public class Subreddit
    {
        private const string SubredditPostUrl = "http://www.reddit.com/r/{0}.json";

        private Reddit Reddit { get; set; }

        public DateTime Created { get; set; }
        public string Description { get; set; }
        public string DisplayName { get; set; }
        public string HeaderImage { get; set; }
        public string HeaderTitle { get; set; }
        public string Id { get; set; }
        public bool NSFW { get; set; }
        public string PublicDescription { get; set; }
        public int Subscribers { get; set; }
        public int ActiveUsers { get; set; }
        public string Title { get; set; }
        public string Url { get; set; }
        public string Name { get; set; }

        private Subreddit()
        {
        }

        protected internal Subreddit(Reddit reddit, JToken json)
        {
            Reddit = reddit;
            var data = json["data"];
            Created = Reddit.UnixTimeStampToDateTime(data["created"].Value<double>());
            Description = data["description"].Value<string>();
            DisplayName = data["display_name"].Value<string>();
            HeaderImage = data["header_img"].Value<string>();
            HeaderTitle = data["header_title"].Value<string>();
            Id = data["name"].Value<string>();
            NSFW = data["over18"].Value<bool>();
            PublicDescription = data["public_description"].Value<string>();
            Subscribers = data["subscribers"].Value<int>();
            Title = data["title"].Value<string>();
            Url = data["url"].Value<string>();
            ActiveUsers = data["accounts_active"].Value<int>();
            Name = Url;
            if (Name.StartsWith("/r/"))
                Name = Name.Substring(3);
            if (Name.StartsWith("r/"))
                Name = Name.Substring(2);
            Name = Name.TrimEnd('/');
        }

        public static Subreddit GetRSlashAll(Reddit reddit)
        {
            var rSlashAll = new Subreddit
            {
                DisplayName = "/r/all",
                Title = "/r/all",
                Url = "/r/all",
                Name = "all",
                Reddit = reddit
            };
            return rSlashAll;
        }

        public Post[] GetPosts()
        {
            var request = Reddit.CreateGet(string.Format(SubredditPostUrl, Name));
            var response = request.GetResponse();
            var data = Reddit.GetResponseString(response.GetResponseStream());
            var json = JObject.Parse(data);
            var posts = new List<Post>();
            var postJson = json["data"]["children"];
            foreach (var post in postJson)
                posts.Add(new Post(Reddit, post));
            return posts.ToArray();
        }
    }
}
