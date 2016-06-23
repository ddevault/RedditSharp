using System;
using System.Threading.Tasks;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace RedditSharp.Models
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

        public new AuthenticatedUser Init(Reddit reddit, JToken json, IWebAgent webAgent)
        {
            CommonInit(reddit, json, webAgent);
            JsonConvert.PopulateObject(json["name"] == null ? json["data"].ToString() : json.ToString(), this,
                reddit.JsonSerializerSettings);
            return this;
        }
        public async new Task<AuthenticatedUser> InitAsync(Reddit reddit, JToken json, IWebAgent webAgent)
        {
            CommonInit(reddit, json, webAgent);
            await Task.Factory.StartNew(() => JsonConvert.PopulateObject(json["name"] == null ? json["data"].ToString() : json.ToString(), this,
                reddit.JsonSerializerSettings));
            return this;
        }

        private void CommonInit(Reddit reddit, JToken json, IWebAgent webAgent)
        {
            base.Init(reddit, json, webAgent);
        }

        public Listing<Subreddit> ModeratorSubreddits
        {
            get
            {
                return new Listing<Subreddit>(Reddit, ModeratorUrl, WebAgent);
            }
        }

        public Listing<Model> UnreadMessages
        {
            get
            {
                return new Listing<Model>(Reddit, UnreadMessagesUrl, WebAgent);
            }
        }

        public Listing<VotableModel> ModerationQueue
        {
            get
            {
                return new Listing<VotableModel>(Reddit, ModQueueUrl, WebAgent);
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
        public Listing<Model> GetUnreadMessages()
        {
            return UnreadMessages;
        }

        [Obsolete("Use ModerationQueue property instead")]
        public Listing<VotableModel> GetModerationQueue()
        {
            return new Listing<VotableModel>(Reddit, ModQueueUrl, WebAgent);
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
