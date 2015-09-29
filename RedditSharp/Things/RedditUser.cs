using System;
using System.Threading.Tasks;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace RedditSharp.Things
{
    public class RedditUser : Thing
    {
        private const string OverviewUrl = "/user/{0}.json";
        private const string CommentsUrl = "/user/{0}/comments.json";
        private const string LinksUrl = "/user/{0}/submitted.json";
        private const string SubscribedSubredditsUrl = "/subreddits/mine.json";
        private const string LikedUrl = "/user/{0}/liked.json";
        private const string DislikedUrl = "/user/{0}/disliked.json";

        private const int MAX_LIMIT = 100;

        public RedditUser Init(Reddit reddit, JToken json, IWebAgent webAgent)
        {
            CommonInit(reddit, json, webAgent);
            JsonConvert.PopulateObject(json["name"] == null ? json["data"].ToString() : json.ToString(), this,
                reddit.JsonSerializerSettings);
            return this;
        }
        public async Task<RedditUser> InitAsync(Reddit reddit, JToken json, IWebAgent webAgent)
        {
            CommonInit(reddit, json, webAgent);
            await Task.Factory.StartNew(() => JsonConvert.PopulateObject(json["name"] == null ? json["data"].ToString() : json.ToString(), this,
                reddit.JsonSerializerSettings));
            return this;
        }

        private void CommonInit(Reddit reddit, JToken json, IWebAgent webAgent)
        {
            base.Init(json);
            Reddit = reddit;
            WebAgent = webAgent;
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

        public Listing<VotableThing> Overview
        {
            get
            {
                return new Listing<VotableThing>(Reddit, string.Format(OverviewUrl, Name), WebAgent);
            }
        }

        public Listing<Post> LikedPosts
        {
            get
            {
                return new Listing<Post>(Reddit, string.Format(LikedUrl, Name), WebAgent);
            }
        }

        public Listing<Post> DislikedPosts
        {
            get
            {
                return new Listing<Post>(Reddit, string.Format(DislikedUrl, Name), WebAgent);
            }
        }

        public Listing<Comment> Comments
        {
            get
            {
                return new Listing<Comment>(Reddit, string.Format(CommentsUrl, Name), WebAgent);
            }
        }

        public Listing<Post> Posts
        {
            get
            {
                return new Listing<Post>(Reddit, string.Format(LinksUrl, Name), WebAgent);
            }
        }

        public Listing<Subreddit> SubscribedSubreddits
        {
            get
            {
                return new Listing<Subreddit>(Reddit, SubscribedSubredditsUrl, WebAgent);
            }
        }

        /// <summary>
        /// Get a listing of comments and posts from the user sorted by <paramref name="sorting"/>, from time <paramref name="fromTime"/>
        /// and limited to <paramref name="limit"/>.
        /// </summary>
        /// <param name="sorting">How to sort the comments (hot, new, top, controversial).</param>
        /// <param name="limit">How many comments to fetch per request. Max is 100.</param>
        /// <param name="fromTime">What time frame of comments to show (hour, day, week, month, year, all).</param>
        /// <returns>The listing of comments requested.</returns>
        public Listing<VotableThing> GetOverview(Sort sorting = Sort.New, int limit = 25, FromTime fromTime = FromTime.All)
        {
            if ((limit < 1) || (limit > MAX_LIMIT))
                throw new ArgumentOutOfRangeException("limit", "Valid range: [1," + MAX_LIMIT + "]");
            string overviewUrl = string.Format(OverviewUrl, Name);
            overviewUrl += string.Format("?sort={0}&limit={1}&t={2}", Enum.GetName(typeof(Sort), sorting), limit, Enum.GetName(typeof(FromTime), fromTime));

            return new Listing<VotableThing>(Reddit, overviewUrl, WebAgent);
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

        public override string ToString()
        {
            return Name;
        }

        #region Obsolete Getter Methods

        [Obsolete("Use Overview property instead")]
        public Listing<VotableThing> GetOverview()
        {
            return Overview;
        }

        [Obsolete("Use Comments property instead")]
        public Listing<Comment> GetComments()
        {
            return Comments;
        }

        [Obsolete("Use Posts property instead")]
        public Listing<Post> GetPosts()
        {
            return Posts;
        }

        [Obsolete("Use SubscribedSubreddits property instead")]
        public Listing<Subreddit> GetSubscribedSubreddits()
        {
            return SubscribedSubreddits;
        }

        #endregion Obsolete Getter Methods
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
