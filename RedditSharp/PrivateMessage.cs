using System;
using System.Collections.Generic;
using System.Linq;
using Newtonsoft.Json.Linq;
using System.Security.Authentication;
using Newtonsoft.Json;

namespace RedditSharp
{
    public class PrivateMessage : Thing
    {
        private const string SetAsReadUrl = "/api/read_message";
        private const string CommentUrl = "/api/comment";

        private Reddit Reddit { get; set; }
        private IWebAgent WebAgent { get; set; }

        [JsonProperty("body")]
        public string Body { get; set; }

        [JsonProperty("body_html")]
        public string BodyHtml { get; set; }

        [JsonProperty("was_comment")]
        public bool IsComment { get; set; }

        [JsonProperty("created")]
        [JsonConverter(typeof(UnixTimestampConverter))]
        public DateTime Sent { get; set; }

        [JsonProperty("created_utc")]
        [JsonConverter(typeof(UnixTimestampConverter))]
        public DateTime SentUTC { get; set; }

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

        [JsonProperty("parent_id")]
        public string ParentID { get; set; }

        [JsonProperty("first_message_name")]
        public string FirstMessageName { get; set; }

        [JsonIgnore]
        public PrivateMessage[] Replies { get; set; }

        [JsonIgnore]
        public PrivateMessage Parent
        {
            get
            {
                if (string.IsNullOrEmpty(ParentID))
                    return null;
                var id = ParentID.Remove(0, 3);
                var listing = new Listing<PrivateMessage>(Reddit, "/message/messages/" + id + ".json", WebAgent);
                var firstMessage = listing.First();
                if (firstMessage.FullName == ParentID)
                    return listing.First();
                else
                    return firstMessage.Replies.First(x => x.FullName == ParentID);
            }
        }

        public Listing<PrivateMessage> Thread
        {
            get
            {
                if (string.IsNullOrEmpty(ParentID))
                    return null;
                var id = ParentID.Remove(0, 3);
                return new Listing<PrivateMessage>(Reddit, "/message/messages/" + id + ".json", WebAgent);
            }
        }

        public PrivateMessage(Reddit reddit, JToken json, IWebAgent webAgent) : base(json)
        {
            Reddit = reddit;
            WebAgent = webAgent;
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
                            replies.Add(new PrivateMessage(reddit, reply, webAgent));
                        Replies = replies.ToArray();
                    }
                }
            }
        }

        #region Obsolete Getter Methods

        [Obsolete("Use Thread property instead")]
        public Listing<PrivateMessage> GetThread()
        {
            return Thread;
        }

        #endregion Obsolete Gettter Methods

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

        public void Reply(string message)
        {
            if (Reddit.User == null)
                throw new AuthenticationException("No user logged in.");
            var request = WebAgent.CreatePost(CommentUrl);
            var stream = request.GetRequestStream();
            WebAgent.WritePostBody(stream, new
            {
                text = message,
                thing_id = FullName,
                uh = Reddit.User.Modhash
            });
            stream.Close();
            var response = request.GetResponse();
            var data = WebAgent.GetResponseString(response.GetResponseStream());
            var json = JObject.Parse(data);
        }
    }
}
