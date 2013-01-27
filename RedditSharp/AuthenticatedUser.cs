using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text;
using Newtonsoft.Json.Linq;

namespace RedditSharp
{
    public class AuthenticatedUser : RedditUser
    {
        private const string ModeratorUrl = "http://api.reddit.com/reddits/mine/moderator";
        private const string UnreadMessagesUrl = "http://api.reddit.com/message/unread?mark=true&limit={0}";

        public AuthenticatedUser(Reddit reddit, JToken json)
            : base(reddit, json)
        {
            Modhash = json["data"]["modhash"].Value<string>();
            HasMail = json["data"]["has_mail"].Value<bool>();
            HasModMail = json["data"]["has_mod_mail"].Value<bool>();
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

        public string Modhash { get; set; }
        public bool HasMail { get; set; }
        public bool HasModMail { get; set; }
    }
}
