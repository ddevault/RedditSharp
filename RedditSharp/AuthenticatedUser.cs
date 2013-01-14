using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Newtonsoft.Json.Linq;

namespace RedditSharp
{
    public class AuthenticatedUser : RedditUser
    {
        private const string ModeratorUrl = "http://api.reddit.com/reddits/mine/moderator";

        public AuthenticatedUser(Reddit reddit, JToken json) : base(reddit, json)
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

        public string Modhash { get; set; }
        public bool HasMail { get; set; }
        public bool HasModMail { get; set; }
    }
}
