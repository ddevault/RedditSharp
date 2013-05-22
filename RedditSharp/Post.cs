using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Authentication;
using System.Text;
using System.Web;
using Newtonsoft.Json.Linq;
using Newtonsoft.Json;

namespace RedditSharp
{
    public class Post : VotableThing
    {
        private const string CommentUrl = "/api/comment";
        private const string RemoveUrl = "/api/remove";
        private const string GetCommentsUrl = "/comments/{0}.json";
        private const string ApproveUrl = "/api/approve";

        [JsonIgnore]
        private Reddit Reddit { get; set; }

        public Post(Reddit reddit, JToken post) : base(reddit, post)
        {
            Reddit = reddit;
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
        [JsonProperty("author_flair_css_class")]
        public string AuthorFlairCssClass { get; set; }
        [JsonProperty("author_flair_text")]
        public string AuthorFlairText { get; set; }
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
        public string Permalink { get; set; }
        [JsonProperty("saved")]
        public bool Saved { get; set; }
        [JsonProperty("score")]
        public int Score { get; set; }
        [JsonProperty("selftext")]
        public string SelfText { get; set; }
        [JsonProperty("selftext_html")]
        public string SelfTextHtml { get; set; }
        [JsonProperty("subreddit")]
        public string Subreddit { get; set; }
        [JsonProperty("thumbnail")]
        public string Thumbnail { get; set; }
        [JsonProperty("title")]
        public string Title { get; set; }
        [JsonProperty("url")]
        public string Url { get; set; }
        [JsonProperty("num_reports")]
        public int Reports { get; set; }

        public Comment Comment(string message)
        {
            if (Reddit.User == null)
                throw new AuthenticationException("No user logged in.");
            var request = Reddit.CreatePost(CommentUrl);
            var stream = request.GetRequestStream();
            Reddit.WritePostBody(stream, new
                {
                    text = message,
                    thing_id = FullName,
                    uh = Reddit.User.Modhash
                });
            stream.Close();
            var response = request.GetResponse();
            var data = Reddit.GetResponseString(response.GetResponseStream());
            var json = JObject.Parse(data);
            var comment = json["jquery"].FirstOrDefault(i => i[0].Value<int>() == 18 && i[1].Value<int>() == 19);
            return new Comment(Reddit, comment[3][0][0]);
        }

        public void Approve()
        {
            var request = Reddit.CreatePost(ApproveUrl);
            var stream = request.GetRequestStream();
            Reddit.WritePostBody(stream, new
            {
                id = FullName,
                uh = Reddit.User.Modhash
            });
            stream.Close();
            var response = request.GetResponse();
            var data = Reddit.GetResponseString(response.GetResponseStream());
        }

        public void Remove()
        {
            var request = Reddit.CreatePost(RemoveUrl);
            var stream = request.GetRequestStream();
            Reddit.WritePostBody(stream, new
            {
                id = FullName,
                spam = false,
                uh = Reddit.User.Modhash
            });
            stream.Close();
            var response = request.GetResponse();
            var data = Reddit.GetResponseString(response.GetResponseStream());
        }

        public void RemoveSpam()
        {
            var request = Reddit.CreatePost(RemoveUrl);
            var stream = request.GetRequestStream();
            Reddit.WritePostBody(stream, new
            {
                id = FullName,
                spam = true,
                uh = Reddit.User.Modhash
            });
            stream.Close();
            var response = request.GetResponse();
            var data = Reddit.GetResponseString(response.GetResponseStream());
        }

        public Comment[] GetComments()
        {
            var comments = new List<Comment>();

            var request = Reddit.CreateGet(string.Format(GetCommentsUrl, Id));
            var response = request.GetResponse();
            var data = Reddit.GetResponseString(response.GetResponseStream());
            var json = JArray.Parse(data);

            var postJson = json.Last()["data"]["children"];
            foreach (var comment in postJson)
                comments.Add(new Comment(Reddit, comment));

            return comments.ToArray();
        }

    }
}
