using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Authentication;
using System.Text;
using Newtonsoft.Json.Linq;

namespace RedditSharp
{
    public class Post
    {
        private const string CommentUrl = "http://www.reddit.com/api/comment";
        private Reddit Reddit { get; set; }

        public Post(Reddit reddit, JToken post)
        {
            Reddit = reddit;

            var data = post["data"];
            Author = data["author"].Value<string>();
            AuthorFlairClass = data["author_flair_css_class"].Value<string>();
            AuthorFlairText = data["author_flair_text"].Value<string>();
            Created = Reddit.UnixTimeStampToDateTime(data["created"].Value<double>());
            Domain = data["domain"].Value<string>();
            Downvotes = data["downs"].Value<int>();
            Edited = data["edited"].Value<bool>();
            Id = data["name"].Value<string>();
            IsSelfPost = data["is_self"].Value<bool>();
            LinkFlairClass = data["link_flair_css_class"].Value<string>();
            LinkFlairText = data["link_flair_text"].Value<string>();
            CommentCount = data["num_comments"].Value<int>();
            NSFW = data["over_18"].Value<bool>();
            Permalink = data["permalink"].Value<string>();
            Saved = data["saved"].Value<bool>();
            Score = data["score"].Value<int>();
            SelfText = data["selftext"].Value<string>();
            SelfTextHtml = data["selftext_html"].Value<string>();
            Subreddit = data["subreddit"].Value<string>();
            Thumbnail = data["thumbnail"].Value<string>();
            Title = data["title"].Value<string>();
            Upvotes = data["ups"].Value<int>();
            Url = data["url"].Value<string>();
        }

        public string Author { get; set; }
        public string AuthorFlairClass { get; set; }
        public string AuthorFlairText { get; set; }
        public DateTime Created { get; set; }
        public string Domain { get; set; }
        public int Downvotes { get; set; }
        public bool Edited { get; set; }
        public string Id { get; set; }
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
        public int Upvotes { get; set; }
        public string Url { get; set; }

        public Comment Comment(string message)
        {
            if (Reddit.User == null)
                throw new AuthenticationException("No user logged in.");
            var request = Reddit.CreatePost(CommentUrl);
            var stream = request.GetRequestStream();
            Reddit.WritePostBody(stream, new
                {
                    text = message,
                    thing_id = Id,
                    uh = Reddit.User.Modhash
                });
            stream.Close();
            var response = request.GetResponse();
            var data = Reddit.GetResponseString(response.GetResponseStream());
            var json = JObject.Parse(data);
            var comment = json["jquery"].FirstOrDefault(i => i[0].Value<int>() == 18 && i[1].Value<int>() == 19);
            return RedditSharp.Comment.FromPost(Reddit, comment[3][0][0]);
        }
    }
}