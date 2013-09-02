using System;
using Newtonsoft.Json.Linq;
using Newtonsoft.Json;

namespace RedditSharp
{
    public class RedditUser : Thing
    {
        private const string OverviewUrl = "/user/{0}.json";
        private const string CommentsUrl = "/user/{0}/comments.json";
        private const string LinksUrl = "/user/{0}/submitted.json";
        private const string SubscribedSubredditsUrl = "/subreddits/mine.json";
        private const string SetFlairUrl = "/api/flair";

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
        }

        public Listing<Comment> GetComments()
        {
            return new Listing<Comment>(Reddit, string.Format(CommentsUrl, Name));
        }

        public Listing<Post> GetPosts()
        {
            return new Listing<Post>(Reddit, string.Format(LinksUrl, Name));
        }

        public Listing<Subreddit> GetSubscribedSubreddits()
        {
            return new Listing<Subreddit>(Reddit, SubscribedSubredditsUrl);
        }

        public override string ToString()
        {
            return Name;
        }
        
        public void SetFlair(string flairText, string flairClass, string subName)
        {
            if (Reddit.User == null)
                throw new Exception("No user logged in.");

            var request = Reddit.CreatePost(SetFlairUrl);
            Reddit.WritePostBody(request.GetRequestStream(), new
            {
                api_type = "json",
                r = subName,
                css_class = flairClass,
                name = Name,
                text = flairText,
                uh = Reddit.User.Modhash
            });
            var response = request.GetResponse();
            var result = Reddit.GetResponseString(response.GetResponseStream());
            var json = JToken.Parse(result);
            if (!json["json"].ToString().Contains("\"errors\": []"))
                throw new Exception("Error editing flair.");
        }
    }
}
