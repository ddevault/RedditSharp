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
        private const string UnreadMessagesUrl = "/message/unread.json?mark=true&limit=25";
        private const string ModQueueUrl = "/r/mod/about/modqueue.json";
        private const string ModMailUrl = "/message/moderator.json";

        public AuthenticatedUser(Reddit reddit, JToken json) : base(reddit, json)
        {
            JsonConvert.PopulateObject(json["data"].ToString(), this, reddit.JsonSerializerSettings);
        }

        public Listing<Subreddit> GetModeratorReddits()
        {
            return new Listing<Subreddit>(Reddit, ModeratorUrl);
        }

        public Listing<PrivateMessage> GetUnreadMessages()
        {
            return new Listing<PrivateMessage>(Reddit, UnreadMessagesUrl);
        }

        public Listing<VotableThing> GetModerationQueue()
        {
            return new Listing<VotableThing>(Reddit, ModQueueUrl);
        }

        public Listing<Post> GetUnmoderatedLinks ()
        {
            return new Listing<Post>(Reddit, "/r/mod/about/unmoderated.json");
        }

        public Listing<PrivateMessage> GetModMail()
        {
            return new Listing<PrivateMessage>(Reddit, ModMailUrl);
        }

        [JsonProperty("modhash")]
        public string Modhash { get; set; }
        [JsonProperty("has_mail")]
        public bool HasMail { get; set; }
        [JsonProperty("has_mod_mail")]
        public bool HasModMail { get; set; }
    }
}
