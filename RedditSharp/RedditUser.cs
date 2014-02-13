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

        private const int MAX_LIMIT = 100;

        public RedditUser(Reddit reddit, JToken json, IWebAgent webAgent) : base(json)
        {
            Reddit = reddit;
            WebAgent = webAgent;
            JsonConvert.PopulateObject(json["data"].ToString(), this, reddit.JsonSerializerSettings);
        }

        [JsonIgnore]
        protected Reddit Reddit { get; set; }

        [JsonIgnore]
        protected IWebAgent WebAgent { get; set; }

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
            return new Listing<VotableThing>(Reddit, string.Format(OverviewUrl, Name), WebAgent);
        }

        public Listing<Comment> GetComments()
        {
            return new Listing<Comment>(Reddit, string.Format(CommentsUrl, Name), WebAgent);
        }

        /// <summary>
        /// Get a listing of comments from the user sorted by <paramref name="sorting"/>, from time <paramref name="fromTime"/>
        /// and limited to <paramref name="limit"/>.
        /// </summary>
        /// <param name="sorting">How to sort the comments (hot, new, top, controversial).</param>
        /// <param name="limit">How many comments to fetch per request. Max is 100.</param>
        /// <param name="fromTime">What time frame of comments to show (hour, day, week, month, year, all).</param>
        /// <returns>The listing of comments requested.</returns>
        public Listing<Comment> GetComments(Sort sorting = Sort.New, int limit = 25, FromTime fromTime = FromTime.All)
        {
            if ((limit < 1) || (limit > MAX_LIMIT))
                throw new ArgumentOutOfRangeException("limit", "Valid range: [1," + MAX_LIMIT + "]");
            string commentsUrl = string.Format(CommentsUrl, Name);
            commentsUrl += string.Format("?sort={0}&limit={1}&t={2}", Enum.GetName(typeof(Sort), sorting), limit, Enum.GetName(typeof(FromTime), fromTime));

            return new Listing<Comment>(Reddit, commentsUrl, WebAgent);
        }

        public Listing<Post> GetPosts()
        {
            return new Listing<Post>(Reddit, string.Format(LinksUrl, Name), WebAgent);
        }

        /// <summary>
        /// Get a listing of posts from the user sorted by <paramref name="sorting"/>, from time <paramref name="fromTime"/>
        /// and limited to <paramref name="limit"/>.
        /// </summary>
        /// <param name="sorting">How to sort the posts (hot, new, top, controversial).</param>
        /// <param name="limit">How many posts to fetch per request. Max is 100.</param>
        /// <param name="fromTime">What time frame of posts to show (hour, day, week, month, year, all).</param>
        /// <returns>The listing of posts requested.</returns>
        public Listing<Post> GetPosts(Sort sorting = Sort.New, int limit = 25, FromTime fromTime = FromTime.All)
        {
            if ((limit < 1) || (limit > 100))
               throw new ArgumentOutOfRangeException("limit", "Valid range: [1,100]");
            string linksUrl = string.Format(LinksUrl, Name);
            linksUrl += string.Format("?sort={0}&limit={1}&t={2}", Enum.GetName(typeof(Sort), sorting), limit, Enum.GetName(typeof(FromTime), fromTime));

            return new Listing<Post>(Reddit, linksUrl, WebAgent);
        }

        public Listing<Subreddit> GetSubscribedSubreddits()
        {
            return new Listing<Subreddit>(Reddit, SubscribedSubredditsUrl, WebAgent);
        }

        public override string ToString()
        {
            return Name;
        }
    }

    public enum Sort
    {
        New,
        Hot,
        Top,
        Controversial
    }

    public enum FromTime
    {
        All,
        Year,
        Month,
        Week,
        Day,
        Hour
    }
}
