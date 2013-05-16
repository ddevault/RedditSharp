using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Newtonsoft.Json.Linq;
using System.Security.Authentication;
using System.Net;

namespace RedditSharp
{
    public class PrivateMessage : Thing
    {
        private const string SetAsReadUrl = "/api/read_message";
        private const string CommentUrl = "/api/comment";

        private Reddit Reddit { get; set; }

        public string Body { get; set; }
        public bool IsComment { get; set; }
        public DateTime Sent { get; set; }
        public string Destination { get; set; }
        public string Author { get; set; }
        public string BodyHtml { get; set; }
        public string Subreddit { get; set; }
        public bool Unread { get; set; }
        public string Subject { get; set; }

        public PrivateMessage(Reddit reddit, JToken json) : base(json)
        {
            Reddit = reddit;
            var data = json["data"];

            Body = data["body"].Value<string>();
            IsComment = data["was_comment"].Value<bool>();
            Sent = Reddit.UnixTimeStampToDateTime(data["created"].Value<double>());
            Destination = data["dest"].Value<string>();
            Author = data["author"].Value<string>();
            BodyHtml = data["body_html"].Value<string>();
            Subreddit = data["subreddit"].Value<string>();
            Unread = data["new"].Value<bool>();
            Subject = data["subject"].Value<string>();
        }

        public void SetAsRead()
        {
            var request = Reddit.CreatePost(SetAsReadUrl);
            Reddit.WritePostBody(request.GetRequestStream(), new
            {
                id = this.FullName,
                uh = Reddit.User.Modhash,
                api_type = "json"
            });
            var response = request.GetResponse();
            var data = Reddit.GetResponseString(response.GetResponseStream());
        }

        public void Reply(string message)
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
        }
    }
}
