using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Security.Authentication;
using System.Threading.Tasks;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace RedditSharp.Things
{
    public class Comment : VotableThing
    {
        private const string CommentUrl = "/api/comment";
        private const string EditUserTextUrl = "/api/editusertext";
        private const string RemoveUrl = "/api/remove";
        private const string DelUrl = "/api/del";
        private const string SetAsReadUrl = "/api/read_message";

        [JsonIgnore]
        private Reddit Reddit { get; set; }
        [JsonIgnore]
        private IWebAgent WebAgent { get; set; }

        public Comment Init(Reddit reddit, JToken json, IWebAgent webAgent, Thing sender)
        {
            var data = CommonInit(reddit, json, webAgent, sender);
            ParseComments(reddit, json, webAgent, sender);
            JsonConvert.PopulateObject(data.ToString(), this, reddit.JsonSerializerSettings);
            return this;
        }
        public async Task<Comment> InitAsync(Reddit reddit, JToken json, IWebAgent webAgent, Thing sender)
        {
            var data = CommonInit(reddit, json, webAgent, sender);
            await ParseCommentsAsync(reddit, json, webAgent, sender);
            await Task.Factory.StartNew(() => JsonConvert.PopulateObject(data.ToString(), this, reddit.JsonSerializerSettings));
            return this;
        }

        private JToken CommonInit(Reddit reddit, JToken json, IWebAgent webAgent, Thing sender)
        {
            base.Init(reddit, webAgent, json);
            var data = json["data"];
            Reddit = reddit;
            WebAgent = webAgent;
            this.Parent = sender;

            // Handle Reddit's API being horrible
            if (data["context"] != null)
            {
                var context = data["context"].Value<string>();
                LinkId = context.Split('/')[4];
            }
         
            return data;
        }

        private void ParseComments(Reddit reddit, JToken data, IWebAgent webAgent, Thing sender)
        {
            // Parse sub comments
            var replies = data["data"]["replies"];
            var subComments = new List<Comment>();
            if (replies != null && replies.Count() > 0)
            {
                foreach (var comment in replies["data"]["children"])
                    subComments.Add(new Comment().Init(reddit, comment, webAgent, sender));
            }
            Comments = subComments.ToArray();
        }

        private async Task ParseCommentsAsync(Reddit reddit, JToken data, IWebAgent webAgent, Thing sender)
        {
            // Parse sub comments
            var replies = data["data"]["replies"];
            var subComments = new List<Comment>();
            if (replies != null && replies.Count() > 0)
            {
                foreach (var comment in replies["data"]["children"])
                    subComments.Add(await new Comment().InitAsync(reddit, comment, webAgent, sender));
            }
            Comments = subComments.ToArray();            
        }

        [JsonProperty("author")]
        public string Author { get; set; }
        [JsonProperty("banned_by")]
        public string BannedBy { get; set; }
        [JsonProperty("body")]
        public string Body { get; set; }
        [JsonProperty("body_html")]
        public string BodyHtml { get; set; }
        [JsonProperty("parent_id")]
        public string ParentId { get; set; }
        [JsonProperty("subreddit")]
        public string Subreddit { get; set; }
        [JsonProperty("approved_by")]
        public string ApprovedBy { get; set; }
        [JsonProperty("author_flair_css_class")]
        public string AuthorFlairCssClass { get; set; }
        [JsonProperty("author_flair_text")]
        public string AuthorFlairText { get; set; }
        [JsonProperty("gilded")]
        public int Gilded { get; set; }
        [JsonProperty("link_id")]
        public string LinkId { get; set; }
        [JsonProperty("link_title")]
        public string LinkTitle { get; set; }
        [JsonProperty("num_reports")]
        public int? NumReports { get; set; }

        [JsonIgnore]
        public IList<Comment> Comments { get; private set; }

        [JsonIgnore]
        public Thing Parent { get; internal set; }

        public override string Shortlink
        {
            get
            {
                // Not really a "short" link, but you can't actually use short links for comments
                string linkId = "";
                int index = this.LinkId.IndexOf('_');
                if (index > -1)
                {
                    linkId = this.LinkId.Substring(index + 1);
                }

                return String.Format("{0}://{1}/r/{2}/comments/{3}/_/{4}",
                                     RedditSharp.WebAgent.Protocol, RedditSharp.WebAgent.RootDomain,
                                     this.Subreddit, this.Parent != null ? this.Parent.Id : linkId, this.Id);
            }
        }

        public Comment Reply(string message)
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
                //r = Subreddit
            });
            stream.Close();
            try
            {
                var response = request.GetResponse();
                var data = WebAgent.GetResponseString(response.GetResponseStream());
                var json = JObject.Parse(data);
                if (json["json"]["ratelimit"] != null)
                    throw new RateLimitException(TimeSpan.FromSeconds(json["json"]["ratelimit"].ValueOrDefault<double>()));
                return new Comment().Init(Reddit, json["json"]["data"]["things"][0], WebAgent, this);
            }
            catch (WebException ex)
            {
                var error = new StreamReader(ex.Response.GetResponseStream()).ReadToEnd();
                return null;
            }
        }

        /// <summary>
        /// Replaces the text in this comment with the input text.
        /// </summary>
        /// <param name="newText">The text to replace the comment's contents</param>        
        public void EditText(string newText)
        {
            if (Reddit.User == null)
                throw new Exception("No user logged in.");

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
                Body = newText;
            else
                throw new Exception("Error editing text.");
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

        public void Del()
        {
            var data = SimpleAction(DelUrl);
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

        public void SetAsRead()
        {
            var request = WebAgent.CreatePost(SetAsReadUrl);
            WebAgent.WritePostBody(request.GetRequestStream(), new
                                 {
                                     id = FullName,
                                     uh = Reddit.User.Modhash,
                                     api_type = "json"
                                 });
            var response = request.GetResponse();
            var data = WebAgent.GetResponseString(response.GetResponseStream());
        }
    }
}
