using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Newtonsoft.Json.Linq;
using System.Security.Authentication;
using System.Net;
using Newtonsoft.Json;

namespace RedditSharp
{
    public class PrivateMessage : Thing
    {
        private const string SetAsReadUrl = "/api/read_message";
        private const string CommentUrl = "/api/comment";

        private Reddit Reddit { get; set; }

        [JsonProperty("body")]
        public string Body { get; set; }
        [JsonProperty("body_html")]
        public string BodyHtml { get; set; }
        [JsonProperty("was_comment")]
        public bool IsComment { get; set; }
        [JsonProperty("created")]
        [JsonConverter(typeof(UnixTimestampConverter))]
        public DateTime Sent { get; set; }
        [JsonProperty("dest")]
        public string Destination { get; set; }
        [JsonProperty("author")]
        public string Author { get; set; }
        [JsonProperty("subreddit")]
        public string Subreddit { get; set; }
        [JsonProperty("new")]
        public bool Unread { get; set; }
        [JsonProperty("subject")]
        public string Subject { get; set; }
        [JsonIgnore]
        public PrivateMessage[] Replies { get; set; }

        public PrivateMessage(Reddit reddit, JToken json) : base(json)
        {
            Reddit = reddit;
            JsonConvert.PopulateObject(json["data"].ToString(), this, reddit.JsonSerializerSettings);
            var data = json["data"];
            if (data["replies"] != null && data["replies"].Any())
            {
                if (data["replies"]["data"] != null)
                {
                    if (data["replies"]["data"]["children"] != null)
                    {
                        var replies = new List<PrivateMessage>();
                        foreach (var reply in data["replies"]["data"]["children"])
                            replies.Add(new PrivateMessage(reddit, reply));
                        Replies = replies.ToArray();
                    }
                }
            }
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
