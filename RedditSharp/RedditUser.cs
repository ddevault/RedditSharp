using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Newtonsoft.Json.Linq;

namespace RedditSharp
{
    public class RedditUser
    {
        private const string OverviewUrl = "/user/{0}.json";
        private const string CommentsUrl = "/user/{0}/comments.json";
        private const string LinksUrl = "/user/{0}/submitted.json";

        public RedditUser(Reddit reddit, JToken json)
        {
            var data = json["data"];
            Name = data["name"].ValueOrDefault<string>();
            Id = data["id"].ValueOrDefault<string>();
            HasGold = data["is_gold"].ValueOrDefault<bool>();
            IsModerator = data["is_mod"].ValueOrDefault<bool>();
            LinkKarma = data["link_karma"].ValueOrDefault<int>();
            CommentKarma = data["comment_karma"].ValueOrDefault<int>();
            Created = Reddit.UnixTimeStampToDateTime(data["created"].ValueOrDefault<double>());
            Reddit = reddit;
        }

        protected Reddit Reddit { get; set; }

        public string Name { get; set; }
        public string Id { get; set; }
        public bool HasGold { get; set; }
        public bool IsModerator { get; set; }
        public int LinkKarma { get; set; }
        public int CommentKarma { get; set; }
        public DateTime Created { get; set; }

        public VotableThing[] GetOverview()
        {
            var request = Reddit.CreateGet(string.Format(OverviewUrl, Name));
            var response = request.GetResponse();
            var data = Reddit.GetResponseString(response.GetResponseStream());
            var json = JToken.Parse(data);
            var items = new List<VotableThing>();
            foreach (var item in json["data"]["children"])
            {
                if (item["kind"].Value<string>() == "t1")
                    items.Add(new Comment(Reddit, item));
                else
                    items.Add(new Post(Reddit, item));
            }
            return items.ToArray();
        }

        public Comment[] GetComments()
        {
            var request = Reddit.CreateGet(string.Format(CommentsUrl, Name));
            var response = request.GetResponse();
            var data = Reddit.GetResponseString(response.GetResponseStream());
            var json = JToken.Parse(data);
            var items = new List<Comment>();
            foreach (var item in json["data"]["children"])
                items.Add(new Comment(Reddit, item));
            return items.ToArray();
        }

        public Post[] GetPosts()
        {
            var request = Reddit.CreateGet(string.Format(LinksUrl, Name));
            var response = request.GetResponse();
            var data = Reddit.GetResponseString(response.GetResponseStream());
            var json = JToken.Parse(data);
            var items = new List<Post>();
            foreach (var item in json["data"]["children"])
                items.Add(new Post(Reddit, item));
            return items.ToArray();
        }

        public override string ToString()
        {
            return this.Name;
        }
    }
}
