using Newtonsoft.Json.Linq;
using Newtonsoft.Json;

namespace RedditSharp
{
    public class AuthenticatedUser : RedditUser
    {
        private const string ModeratorUrl = "/reddits/mine/moderator.json";
        private const string UnreadMessagesUrl = "/message/unread.json?mark=true&limit=25";
        private const string ModQueueUrl = "/r/mod/about/modqueue.json";
        private const string UnmoderatedUrl = "/r/mod/about/unmoderated.json";
        private const string ModMailUrl = "/message/moderator.json";

        public AuthenticatedUser(Reddit reddit, JToken json, IWebAgent webAgent) : base(reddit, json, webAgent)
        {
            JsonConvert.PopulateObject(json["data"].ToString(), this, reddit.JsonSerializerSettings);
        }

        public Listing<Subreddit> GetModeratorReddits()
        {
            return new Listing<Subreddit>(Reddit, ModeratorUrl, WebAgent);
        }

        public Listing<Thing> GetUnreadMessages()
        {
            return new Listing<Thing>(Reddit, UnreadMessagesUrl, WebAgent);
        }

        public Listing<VotableThing> GetModerationQueue()
        {
            return new Listing<VotableThing>(Reddit, ModQueueUrl, WebAgent);
        }

        public Listing<Post> GetUnmoderatedLinks ()
        {
            return new Listing<Post>(Reddit, UnmoderatedUrl, WebAgent);
        }

        public Listing<PrivateMessage> GetModMail()
        {
            return new Listing<PrivateMessage>(Reddit, ModMailUrl, WebAgent);
        }

        [JsonProperty("modhash")]
        public string Modhash { get; set; }
        [JsonProperty("has_mail")]
        public bool HasMail { get; set; }
        [JsonProperty("has_mod_mail")]
        public bool HasModMail { get; set; }
    }
}
