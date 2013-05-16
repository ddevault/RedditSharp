using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Authentication;
using System.Text;
using System.Web;
using Newtonsoft.Json.Linq;

namespace RedditSharp
{
    public class Post : VotableThing
    {
        private const string CommentUrl = "/api/comment";
        private const string RemoveUrl = "/api/remove";
        private const string GetCommentsUrl = "/comments/{0}.json";
        private const string ApproveUrl = "/api/approve";

        private Reddit Reddit { get; set; }

        public Post(Reddit reddit, JToken post) : base(reddit, post)
        {
            Reddit = reddit;

            var data = post["data"];
            AuthorName = data["author"].ValueOrDefault<string>();
            AuthorFlairClass = data["author_flair_css_class"].ValueOrDefault<string>();
            AuthorFlairText = data["author_flair_text"].ValueOrDefault<string>();
            Domain = data["domain"].ValueOrDefault<string>();
            Edited = data["edited"].ValueOrDefault<bool>();
            IsSelfPost = data["is_self"].ValueOrDefault<bool>();
            LinkFlairClass = data["link_flair_css_class"].ValueOrDefault<string>();
            LinkFlairText = data["link_flair_text"].ValueOrDefault<string>();
            CommentCount = data["num_comments"].ValueOrDefault<int>();
            NSFW = data["over_18"].ValueOrDefault<bool>();
            Permalink = data["permalink"].ValueOrDefault<string>();
            Saved = data["saved"].ValueOrDefault<bool>();
            Score = data["score"].ValueOrDefault<int>();
            SelfText = data["selftext"].ValueOrDefault<string>();
            SelfTextHtml = data["selftext_html"].ValueOrDefault<string>();
            Subreddit = data["subreddit"].ValueOrDefault<string>();
            Thumbnail = data["thumbnail"].ValueOrDefault<string>();
            Title = HttpUtility.HtmlDecode(data["title"].ValueOrDefault<string>() ?? string.Empty);
            Url = data["url"].ValueOrDefault<string>();

            if (data["num_reports"] != null)
            {
                var reports = data["num_reports"].ValueOrDefault<string>();
                int value;
                if (reports != null)
                {
                    if (int.TryParse(reports, out value))
                        Reports = value;
                }
            }
        }

        public string AuthorName { get; set; }
        public RedditUser Author
        {
            get
            {
                return Reddit.GetUser(AuthorName);
            }
        }

        public string AuthorFlairClass { get; set; }
        public string AuthorFlairText { get; set; }
        public string Domain { get; set; }
        public bool Edited { get; set; }
        public bool IsSelfPost { get; set; }
        public string LinkFlairClass { get; set; }
        public string LinkFlairText { get; set; }
        public int CommentCount { get; set; }
        public bool NSFW { get; set; }
        public string Permalink { get; set; }
        public bool Saved { get; set; }
        public int Score { get; set; }
        public string SelfText { get; set; }
        public string SelfTextHtml { get; set; }
        public string Subreddit { get; set; }
        public string Thumbnail { get; set; }
        public string Title { get; set; }
        public string Url { get; set; }

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

        public List<Comment> GetComments()
        {
            var comments = new List<Comment>();

            var request = Reddit.CreateGet(string.Format(GetCommentsUrl, Id));
            var response = request.GetResponse();
            var data = Reddit.GetResponseString(response.GetResponseStream());
            var json = JArray.Parse(data);

            var postJson = json.Last()["data"]["children"];
            foreach (var comment in postJson)
                comments.Add(new Comment(Reddit, comment));

            return comments;
        }

    }
}
