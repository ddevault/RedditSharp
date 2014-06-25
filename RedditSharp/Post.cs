using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Authentication;
using Newtonsoft.Json.Linq;
using Newtonsoft.Json;

namespace RedditSharp
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
        private const string SetFlairUrl = "/api/flair";
        private const string MarkNSFWUrl = "/api/marknsfw";
        private const string UnmarkNSFWUrl = "/api/unmarknsfw";

        [JsonIgnore]
        private Reddit Reddit { get; set; }

        [JsonIgnore]
        private IWebAgent WebAgent { get; set; }

        public Post(Reddit reddit, JToken post, IWebAgent webAgent) : base(reddit, webAgent, post)
        {
            Reddit = reddit;
            WebAgent = webAgent;
            JsonConvert.PopulateObject(post["data"].ToString(), this, reddit.JsonSerializerSettings);
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
                var comments = new List<Comment>();

                var request = WebAgent.CreateGet(string.Format(GetCommentsUrl, Id));
                var response = request.GetResponse();
                var data = WebAgent.GetResponseString(response.GetResponseStream());
                var json = JArray.Parse(data);

                var postJson = json.Last()["data"]["children"];
                foreach (var comment in postJson)
                    comments.Add(new Comment(Reddit, comment, WebAgent, this));

                return comments.ToArray();
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

        [JsonProperty("subreddit")]
        public string Subreddit { get; set; }

        [JsonProperty("thumbnail")]
        [JsonConverter(typeof(UrlParser))]
        public Uri Thumbnail { get; set; }

        [JsonProperty("title")]
        public string Title { get; set; }

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
            var comment = json["json"]["data"]["things"][0];
            return new Comment(Reddit, comment, WebAgent, this);
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

        public void Approve()
        {
            var data = SimpleAction(ApproveUrl);
        }

        public void Remove()
        {
            var data = SimpleAction(RemoveUrl);
        }

        public void RemoveSpam()
        {
            var request = WebAgent.CreatePost(RemoveUrl);
            var stream = request.GetRequestStream();
            WebAgent.WritePostBody(stream, new
            {
                id = FullName,
                spam = true,
                uh = Reddit.User.Modhash
            });
            stream.Close();
            var response = request.GetResponse();
            var data = WebAgent.GetResponseString(response.GetResponseStream());
        }
        
        public void Del()
        {
            var data = SimpleAction(ApproveUrl);
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

            var request = WebAgent.CreatePost(SetFlairUrl);
            WebAgent.WritePostBody(request.GetRequestStream(), new
            {
                api_type = "json",
                r = Subreddit,
                css_class = flairClass,
                link = FullName,
                //name = Name,
                text = flairText,
                uh = Reddit.User.Modhash
            });
            var response = request.GetResponse();
            var result = WebAgent.GetResponseString(response.GetResponseStream());
            var json = JToken.Parse(result);
            LinkFlairText = flairText;
        }
    }
}
