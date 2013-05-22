using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text;
using Newtonsoft.Json.Linq;
using Newtonsoft.Json;

namespace RedditSharp
{
    public class AuthenticatedUser : RedditUser
    {
        private const string ModeratorUrl = "/reddits/mine/moderator.json";
        private const string UnreadMessagesUrl = "/message/unread.json?mark=true&limit={0}";
        private const string ModQueueUrl = "/r/mod/about/modqueue.json";
        private const string ModMailUrl = "/message/moderator.json";

        public AuthenticatedUser(Reddit reddit, JToken json) : base(reddit, json)
        {
            JsonConvert.PopulateObject(json["data"].ToString(), this, reddit.JsonSerializerSettings);
        }

        public Subreddit[] GetModeratorReddits()
        {
            var reddits = new List<Subreddit>();
            var request = Reddit.CreateGet(ModeratorUrl);
            var response = request.GetResponse();
            var result = Reddit.GetResponseString(response.GetResponseStream());
            var json = JToken.Parse(result);
            foreach (var subreddit in json["data"]["children"])
                reddits.Add(new Subreddit(Reddit, subreddit));
            return reddits.ToArray();
        }

        public PrivateMessage[] GetUnreadMessages(int limit = 25)
        {
            var messages = new List<PrivateMessage>();
            var request = Reddit.CreateGet(string.Format(UnreadMessagesUrl, limit));
            var response = (HttpWebResponse)request.GetResponse();
            var result = Reddit.GetResponseString(response.GetResponseStream());
            var json = JToken.Parse(result);
            foreach (var message in json["data"]["children"])
                messages.Add(new PrivateMessage(Reddit, message));
            return messages.ToArray();
        }

        public VotableThing[] GetModerationQueue()
        {
            var request = Reddit.CreateGet(ModQueueUrl);
            var response = request.GetResponse();
            var result = Reddit.GetResponseString(response.GetResponseStream());
            var json = JToken.Parse(result);
            var items = new List<VotableThing>();
            foreach (var item in json["data"]["children"])
            {
                if (item["kind"].Value<string>() == "t3") // TODO: Maybe add a method to parse Things based on the kind?
                    items.Add(new Post(Reddit, item));
                else if (item["kind"].Value<string>() == "t1")
                    items.Add(new Comment(Reddit, item));
            }
            return items.ToArray();
        }

        public PrivateMessage[] GetModMail()
        {
            var request = Reddit.CreateGet(ModMailUrl);
            var response = request.GetResponse();
            var result = Reddit.GetResponseString(response.GetResponseStream());
            var json = JToken.Parse(result);
            var items = new List<PrivateMessage>();
            foreach (var item in json["data"]["children"])
                items.Add(new PrivateMessage(Reddit, item));
            return items.ToArray();
        }

        [JsonProperty("modhash")]
        public string Modhash { get; set; }
        [JsonProperty("has_mail")]
        public bool HasMail { get; set; }
        [JsonProperty("has_mod_mail")]
        public bool HasModMail { get; set; }
    }
}
