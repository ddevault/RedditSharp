using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Newtonsoft.Json.Linq;
using Newtonsoft.Json;

namespace RedditSharp
{
    public class RedditUser : Thing
    {
        private const string OverviewUrl = "/user/{0}.json";
        private const string CommentsUrl = "/user/{0}/comments.json";
        private const string LinksUrl = "/user/{0}/submitted.json";

        public RedditUser(Reddit reddit, JToken json) : base(json)
        {
            Reddit = reddit;
            JsonConvert.PopulateObject(json["data"].ToString(), this, reddit.JsonSerializerSettings);
        }

        [JsonIgnore]
        protected Reddit Reddit { get; set; }

        [JsonProperty("name")]
        public string Name { get; set; }
        [JsonProperty("is_gold")]
        public bool HasGold { get; set; }
        [JsonProperty("is_mod")]
        public bool IsModerator { get; set; }
        [JsonProperty("link_karma")]
        public int LinkKarma { get; set; }
        [JsonProperty("comment_karma")]
        public int CommentKarma { get; set; }
        [JsonProperty("created")]
        [JsonConverter(typeof(UnixTimestampConverter))]
        public DateTime Created { get; set; }

        public Listing<VotableThing> GetOverview()
        {
            return new Listing<VotableThing>(Reddit, string.Format(OverviewUrl, Name));
            //var request = Reddit.CreateGet(string.Format(OverviewUrl, Name));
            //var response = request.GetResponse();
            //var data = Reddit.GetResponseString(response.GetResponseStream());
            //var json = JToken.Parse(data);
            //var items = new List<VotableThing>();
            //foreach (var item in json["data"]["children"])
            //{
            //    if (item["kind"].Value<string>() == "t1")
            //        items.Add(new Comment(Reddit, item));
            //    else
            //        items.Add(new Post(Reddit, item));
            //}
            //return items.ToArray();
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
