using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Authentication;
using System.Text;
using Newtonsoft.Json.Linq;

namespace RedditSharp
{
    public class Comment
    {
        private const string CommentUrl = "http://www.reddit.com/api/comment";
        private Reddit Reddit { get; set; }

        internal static Comment FromPost(Reddit reddit, JToken json)
        {
            var comment = new Comment();
            var data = json["data"];
            comment.Content = data["contentText"].Value<string>();
            comment.ContentHtml = data["contentHTML"].Value<string>();
            comment.Id = data["id"].Value<string>();
            comment.Reddit = reddit;
            return comment;
        }

        private Comment()
        {
        }

        public Comment(Reddit reddit, JToken json)
        {
            // TODO
        }

        public string Author { get; set; }
        public string Content { get; set; }
        public string ContentHtml { get; set; }
        public string Id { get; set; }
        public string ParentId { get; set; }

        public Comment Reply(string message)
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
            var comment = json["jquery"].FirstOrDefault(i => i[0].Value<int>() == 30 && i[1].Value<int>() == 31);
            return FromPost(Reddit, comment[3][0][0]);
        }
    }
}
