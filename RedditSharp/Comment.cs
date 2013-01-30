using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Authentication;
using System.Text;
using Newtonsoft.Json.Linq;

namespace RedditSharp
{
    public class Comment : VotableThing
    {
        private const string CommentUrl = "http://www.reddit.com/api/comment";
        private const string DistinguishUrl = "http://www.reddit.com/api/distinguish";
        private Reddit Reddit { get; set; }

        public Comment(Reddit reddit, JToken json) : base(reddit, json)
        {
            var data = json["data"];

            Author = data["author"].Value<string>();
            Created = Reddit.UnixTimeStampToDateTime(data["created"].Value<double>());
            Content = data["body"].Value<string>();
            ContentHtml = data["body_html"].Value<string>();
            Downvotes = data["downs"].Value<int>();
            Upvotes = data["ups"].Value<int>();
            Reddit = reddit;

            //Parse sub comments
            Comments = new List<Comment>();
            if (data["replies"] != null && data["replies"].Any())
            {
                foreach (var comment in data["replies"]["data"]["children"])
                    Comments.Add(new Comment(reddit, comment));
            }
        }

        public string Author { get; set; }
        public string Content { get; set; }
        public string ContentHtml { get; set; }
        public string ParentId { get; set; }
        public DateTime Created { get; set; }
        public int Downvotes { get; set; }
        public int Upvotes { get; set; }

        public List<Comment> Comments { get; set; }

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
            return new Comment(Reddit, comment[3][0][0]);
        }

        public void Distinguish(DistinguishType distinguishType)
        {
            if (Reddit.User == null)
                throw new AuthenticationException("No user logged in.");
            var request = Reddit.CreatePost(DistinguishUrl);
            var stream = request.GetRequestStream();
            string how;
            switch (distinguishType)
            {
                case DistinguishType.Admin:
                    how = "admin";
                    break;
                case DistinguishType.Moderator:
                    how = "yes";
                    break;
                case DistinguishType.None:
                    how = "no";
                    break;
                default:
                    how = "special";
                    break;
            }
            Reddit.WritePostBody(stream, new
            {
                how,
                id = Id,
                uh = Reddit.User.Modhash
            });
            stream.Close();
            var response = request.GetResponse();
            var data = Reddit.GetResponseString(response.GetResponseStream());
            var json = JObject.Parse(data);
            if (json["jquery"].Count(i => i[0].Value<int>() == 11 && i[1].Value<int>() == 12) == 0)
                throw new AuthenticationException("You are not permitted to distinguish this comment.");
        }
    }

    public enum DistinguishType
    {
        Moderator,
        Admin,
        Special,
        None
    }
}
