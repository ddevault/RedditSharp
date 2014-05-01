using System;
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
        private const string MessagesUrl = "/message/messages.json";
        private const string InboxUrl = "/message/inbox.json";
        private const string SentUrl = "/message/sent.json";

        public AuthenticatedUser(Reddit reddit, JToken json, IWebAgent webAgent)
            : base(reddit, json, webAgent)
        {
            JsonConvert.PopulateObject(json["name"] == null ? json["data"].ToString() : json.ToString(), this, reddit.JsonSerializerSettings);
        }

        public Listing<Subreddit> ModeratorSubreddits
        {
            get
            {
                return new Listing<Subreddit>(Reddit, ModeratorUrl, WebAgent);
            }
        }

        public Listing<Thing> UnreadMessages
        {
            get
            {
                return new Listing<Thing>(Reddit, UnreadMessagesUrl, WebAgent);
            }
        }

        public Listing<VotableThing> ModerationQueue
        {
            get
            {
                return new Listing<VotableThing>(Reddit, ModQueueUrl, WebAgent);
            }
        }

        public Listing<Post> UnmoderatedLinks
        {
            get
            {
                return new Listing<Post>(Reddit, UnmoderatedUrl, WebAgent);
            }
        }

        public Listing<PrivateMessage> ModMail
        {
            get
            {
                return new Listing<PrivateMessage>(Reddit, ModMailUrl, WebAgent);
            }
        }

        public Listing<PrivateMessage> PrivateMessages
        {
            get
            {
                return new Listing<PrivateMessage>(Reddit, MessagesUrl, WebAgent);
            }
        }

        public Listing<PrivateMessage> Inbox
        {
            get
            {
                return new Listing<PrivateMessage>(Reddit, InboxUrl, WebAgent);
            }
        }

        public Listing<PrivateMessage> Sent
        {
            get
            {
                return new Listing<PrivateMessage>(Reddit, SentUrl, WebAgent);
            }
        }

        #region Obsolete Getter Methods

        [Obsolete("Use ModeratorSubreddits property instead")]
        public Listing<Subreddit> GetModeratorReddits()
        {
            return ModeratorSubreddits;
        }

        [Obsolete("Use UnreadMessages property instead")]
        public Listing<Thing> GetUnreadMessages()
        {
            return UnreadMessages;
        }

        [Obsolete("Use ModerationQueue property instead")]
        public Listing<VotableThing> GetModerationQueue()
        {
            return new Listing<VotableThing>(Reddit, ModQueueUrl, WebAgent);
        }

        public Listing<Post> GetUnmoderatedLinks()
        {
            return new Listing<Post>(Reddit, UnmoderatedUrl, WebAgent);
        }

        [Obsolete("Use ModMail property instead")]
        public Listing<PrivateMessage> GetModMail()
        {
            return new Listing<PrivateMessage>(Reddit, ModMailUrl, WebAgent);
        }

        [Obsolete("Use PrivateMessages property instead")]
        public Listing<PrivateMessage> GetPrivateMessages()
        {
            return new Listing<PrivateMessage>(Reddit, MessagesUrl, WebAgent);
        }

        [Obsolete("Use Inbox property instead")]
        public Listing<PrivateMessage> GetInbox()
        {
            return new Listing<PrivateMessage>(Reddit, InboxUrl, WebAgent);
        }

        #endregion Obsolete Getter Methods

        [JsonProperty("modhash")]
        public string Modhash { get; set; }

        [JsonProperty("has_mail")]
        public bool HasMail { get; set; }

        [JsonProperty("has_mod_mail")]
        public bool HasModMail { get; set; }
    }
}
