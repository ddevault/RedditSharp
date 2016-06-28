using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Authentication;
using System.Threading.Tasks;
using System.Web;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace RedditSharp.Things
{
    public class Post : VotableThing
    {
        private const string CommentUrl = "/api/comment";
        private const string RemoveUrl = "/api/remove";
        private const string DelUrl = "/api/del";
        private const string GetCommentsUrl = "/comments/{0}.json";
        private const string ApproveUrl = "/api/approve";
        private const string EditUserTextUrl = "/api/editusertext";
        private const string HideUrl = "/api/hide";
        private const string UnhideUrl = "/api/unhide";
        private const string SetFlairUrl = "/r/{0}/api/flair";
        private const string MarkNSFWUrl = "/api/marknsfw";
        private const string UnmarkNSFWUrl = "/api/unmarknsfw";
        private const string ContestModeUrl = "/api/set_contest_mode";
        private const string StickyModeUrl = "/api/set_subreddit_sticky";

        [JsonIgnore]
        private Reddit Reddit { get; set; }

        [JsonIgnore]
        private IWebAgent WebAgent { get; set; }

        public Post Init(Reddit reddit, JToken post, IWebAgent webAgent)
        {
            CommonInit(reddit, post, webAgent);
            JsonConvert.PopulateObject(post["data"].ToString(), this, reddit.JsonSerializerSettings);
            return this;
        }
        public async Task<Post> InitAsync(Reddit reddit, JToken post, IWebAgent webAgent)
        {
            CommonInit(reddit, post, webAgent);
            await Task.Factory.StartNew(() => JsonConvert.PopulateObject(post["data"].ToString(), this, reddit.JsonSerializerSettings));
            return this;
        }

        private void CommonInit(Reddit reddit, JToken post, IWebAgent webAgent)
        {
            base.Init(reddit, webAgent, post);
            Reddit = reddit;
            WebAgent = webAgent;
        }

        [JsonProperty("author")]
        public string AuthorName { get; set; }

        [JsonIgnore]
        public RedditUser Author
        {
            get
            {
                return Reddit.GetUser(AuthorName);
            }
        }

        public Comment[] Comments
        {
            get
            {
                return ListComments().ToArray();
            }
        }

        [JsonProperty("approved_by")]
        public string ApprovedBy { get; set; }

        [JsonProperty("author_flair_css_class")]
        public string AuthorFlairCssClass { get; set; }

        [JsonProperty("author_flair_text")]
        public string AuthorFlairText { get; set; }

        [JsonProperty("banned_by")]
        public string BannedBy { get; set; }

        [JsonProperty("domain")]
        public string Domain { get; set; }

        [JsonProperty("edited")]
        public bool Edited { get; set; }

        [JsonProperty("is_self")]
        public bool IsSelfPost { get; set; }

        [JsonProperty("link_flair_css_class")]
        public string LinkFlairCssClass { get; set; }

        [JsonProperty("link_flair_text")]
        public string LinkFlairText { get; set; }

        [JsonProperty("num_comments")]
        public int CommentCount { get; set; }

        [JsonProperty("over_18")]
        public bool NSFW { get; set; }

        [JsonProperty("permalink")]
        [JsonConverter(typeof(UrlParser))]
        public Uri Permalink { get; set; }

        [JsonProperty("score")]
        public int Score { get; set; }

        [JsonProperty("selftext")]
        public string SelfText { get; set; }

        [JsonProperty("selftext_html")]
        public string SelfTextHtml { get; set; }

        [JsonProperty("thumbnail")]
        [JsonConverter(typeof(UrlParser))]
        public Uri Thumbnail { get; set; }

        [JsonProperty("title")]
        public string Title { get; set; }

        [JsonProperty("subreddit")]
        public string SubredditName { get; set; }

        [JsonIgnore]
        public Subreddit Subreddit
        {
            get
            {
                return Reddit.GetSubreddit("/r/" + SubredditName);
            }
        }

        [JsonProperty("url")]
        [JsonConverter(typeof(UrlParser))]
        public Uri Url { get; set; }

        [JsonProperty("num_reports")]
        public int? Reports { get; set; }

        public Comment Comment(string message)
        {
            if (Reddit.User == null)
                throw new AuthenticationException("No user logged in.");
            var request = WebAgent.CreatePost(CommentUrl);
            var stream = request.GetRequestStream();
            WebAgent.WritePostBody(stream, new
                {
                    text = message,
                    thing_id = FullName,
                    uh = Reddit.User.Modhash,
                    api_type = "json"
                });
            stream.Close();
            var response = request.GetResponse();
            var data = WebAgent.GetResponseString(response.GetResponseStream());
            var json = JObject.Parse(data);
            if (json["json"]["ratelimit"] != null)
                throw new RateLimitException(TimeSpan.FromSeconds(json["json"]["ratelimit"].ValueOrDefault<double>()));
            return new Comment().Init(Reddit, json["json"]["data"]["things"][0], WebAgent, this);
        }

        private string SimpleAction(string endpoint)
        {
            if (Reddit.User == null)
                throw new AuthenticationException("No user logged in.");
            var request = WebAgent.CreatePost(endpoint);
            var stream = request.GetRequestStream();
            WebAgent.WritePostBody(stream, new
            {
                id = FullName,
                uh = Reddit.User.Modhash
            });
            stream.Close();
            var response = request.GetResponse();
            var data = WebAgent.GetResponseString(response.GetResponseStream());
            return data;
        }

        private string SimpleActionToggle(string endpoint, bool value, bool requiresModAction = false)
        {
            if (Reddit.User == null)
                throw new AuthenticationException("No user logged in.");

            var modNameList = this.Subreddit.Moderators.Select(b => b.Name).ToList();

            if (requiresModAction && !modNameList.Contains(Reddit.User.Name))
                throw new AuthenticationException(
                    String.Format(
                        @"User {0} is not a moderator of subreddit {1}.", 
                        Reddit.User.Name,
                        this.Subreddit.Name));

            var request = WebAgent.CreatePost(endpoint);
            var stream = request.GetRequestStream();
            WebAgent.WritePostBody(stream, new
            {
                id = FullName,
                state = value,
                uh = Reddit.User.Modhash
            });
            stream.Close();
            var response = request.GetResponse();
            var data = WebAgent.GetResponseString(response.GetResponseStream());
            return data;
        }

        public void Approve()
        {
            var data = SimpleAction(ApproveUrl);
        }

        public void Remove()
        {
            RemoveImpl(false);
        }

        public void RemoveSpam()
        {
            RemoveImpl(true);
        }

        private void RemoveImpl(bool spam)
        {
            var request = WebAgent.CreatePost(RemoveUrl);
            var stream = request.GetRequestStream();
            WebAgent.WritePostBody(stream, new
            {
                id = FullName,
                spam = spam,
                uh = Reddit.User.Modhash
            });
            stream.Close();
            var response = request.GetResponse();
            var data = WebAgent.GetResponseString(response.GetResponseStream());
        }

        public void Del()
        {
            var data = SimpleAction(DelUrl);
        }

        public void Hide()
        {
            var data = SimpleAction(HideUrl);
        }

        public void Unhide()
        {
            var data = SimpleAction(UnhideUrl);
        }

        public void MarkNSFW()
        {
            var data = SimpleAction(MarkNSFWUrl);
        }

        public void UnmarkNSFW()
        {
            var data = SimpleAction(UnmarkNSFWUrl);
        }

        public void ContestMode(bool state)
        {
            var data = SimpleActionToggle(ContestModeUrl, state);
        }

        public void StickyMode(bool state)
        {
            var data = SimpleActionToggle(StickyModeUrl, state, true);
        }

        #region Obsolete Getter Methods

        [Obsolete("Use Comments property instead")]
        public Comment[] GetComments()
        {
            return Comments;
        }

        #endregion Obsolete Getter Methods

        /// <summary>
        /// Replaces the text in this post with the input text.
        /// </summary>
        /// <param name="newText">The text to replace the post's contents</param>
        public void EditText(string newText)
        {
            if (Reddit.User == null)
                throw new Exception("No user logged in.");
            if (!IsSelfPost)
                throw new Exception("Submission to edit is not a self-post.");

            var request = WebAgent.CreatePost(EditUserTextUrl);
            WebAgent.WritePostBody(request.GetRequestStream(), new
            {
                api_type = "json",
                text = newText,
                thing_id = FullName,
                uh = Reddit.User.Modhash
            });
            var response = request.GetResponse();
            var result = WebAgent.GetResponseString(response.GetResponseStream());
            JToken json = JToken.Parse(result);
            if (json["json"].ToString().Contains("\"errors\": []"))
                SelfText = newText;
            else
                throw new Exception("Error editing text.");
        }
        public void Update()
        {
            JToken post = Reddit.GetToken(this.Url);
            JsonConvert.PopulateObject(post["data"].ToString(), this, Reddit.JsonSerializerSettings);
        }

        public void SetFlair(string flairText, string flairClass)
        {
            if (Reddit.User == null)
                throw new Exception("No user logged in.");

            var request = WebAgent.CreatePost(string.Format(SetFlairUrl,SubredditName));
            WebAgent.WritePostBody(request.GetRequestStream(), new
            {
                api_type = "json",
                css_class = flairClass,
                link = FullName,
                name = Reddit.User.Name,
                text = flairText,
                uh = Reddit.User.Modhash
            });
            var response = request.GetResponse();
            var result = WebAgent.GetResponseString(response.GetResponseStream());
            var json = JToken.Parse(result);
            LinkFlairText = flairText;
        }

        public List<Comment> ListComments(int? limit = null)
        {
            var url = string.Format(GetCommentsUrl, Id);

            if (limit.HasValue)
            {
                var query = HttpUtility.ParseQueryString(string.Empty);
                query.Add("limit", limit.Value.ToString());
                url = string.Format("{0}?{1}", url, query);
            }

            var request = WebAgent.CreateGet(url);
            var response = request.GetResponse();
            var data = WebAgent.GetResponseString(response.GetResponseStream());
            var json = JArray.Parse(data);
            var postJson = json.Last()["data"]["children"];

            var comments = new List<Comment>();
            foreach (var comment in postJson)
            {
                comments.Add(new Comment().Init(Reddit, comment, WebAgent, this));
            }

            return comments;
        }
    }
}
