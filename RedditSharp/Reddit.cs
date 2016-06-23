using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using RedditSharp.Models;
using RedditSharp.Workers;
using System;
using System.Linq;
using System.Net;
using System.Threading.Tasks;
using DefaultWebAgent = RedditSharp.WebAgent;

namespace RedditSharp
{
    /// <summary>
    /// Class to communicate with Reddit.com
    /// </summary>
    public class Reddit
    {
        #region Constant Urls
        
        private const string GetModelUrl = "/api/info.json?id={0}";
        private const string GetPostUrl = "{0}.json";
        private const string DomainUrl = "www.reddit.com";
        private const string OAuthDomainUrl = "oauth.reddit.com";
        private const string SearchUrl = "/search.json?q={0}&restrict_sr=off&sort={1}&t={2}";
        private const string UrlSearchPattern = "url:'{0}'";
        private const string NewSubredditsUrl = "/subreddits/new.json";
        private const string PopularSubredditsUrl = "/subreddits/popular.json";
        private const string GoldSubredditsUrl = "/subreddits/gold.json";
        private const string DefaultSubredditsUrl = "/subreddits/default.json";
        private const string SearchSubredditsUrl = "/subreddits/search.json?q={0}";


        #endregion

        #region Static Variables

        static Reddit()
        {
            DefaultWebAgent.UserAgent = "";
            DefaultWebAgent.RateLimit = DefaultWebAgent.RateLimitMode.Pace;
            DefaultWebAgent.Protocol = "https";
            DefaultWebAgent.RootDomain = "www.reddit.com";
        }

        #endregion
        
        internal IWebAgent WebAgent { get; set; }
        /// <summary>
        /// Captcha solver instance to use when solving captchas.
        /// </summary>
        public ICaptchaSolver CaptchaSolver;

        /// <summary>
        /// The authenticated user for this instance.
        /// </summary>
        public AuthenticatedUser User { get; set; }

        /// <summary>
        /// Sets the Rate Limiting Mode of the underlying WebAgent
        /// </summary>
        public DefaultWebAgent.RateLimitMode RateLimit
        {
            get { return DefaultWebAgent.RateLimit; }
            set { DefaultWebAgent.RateLimit = value; }
        }

        internal JsonSerializerSettings JsonSerializerSettings { get; set; }

        /// <summary>
        /// Gets the FrontPage using the current Reddit instance.
        /// </summary>
        public Subreddit FrontPage
        {
            get { return Subreddit.GetFrontPage(this); }
        }

        /// <summary>
        /// Gets /r/All using the current Reddit instance.
        /// </summary>
        public Subreddit RSlashAll
        {
            get { return Subreddit.GetRSlashAll(this); }
        }

        public Reddit()
            : this(true) { }

        public Reddit(bool useSsl)
        {
            DefaultWebAgent defaultAgent = new DefaultWebAgent();

            JsonSerializerSettings = new JsonSerializerSettings
                {
                    CheckAdditionalContent = false,
                    DefaultValueHandling = DefaultValueHandling.Ignore
                };
            DefaultWebAgent.Protocol = useSsl ? "https" : "http";
            WebAgent = defaultAgent;
            CaptchaSolver = new ConsoleCaptchaSolver();
        }

        public Reddit(DefaultWebAgent.RateLimitMode limitMode, bool useSsl = true)
            : this(useSsl)
        {
            DefaultWebAgent.UserAgent = "";
            DefaultWebAgent.RateLimit = limitMode;
            DefaultWebAgent.RootDomain = "www.reddit.com";
        }

        public Reddit(string username, string password, bool useSsl = true)
            : this(useSsl)
        {
            LogIn(username, password, useSsl);
        }

        public Reddit(string accessToken)
            : this(true)
        {
            DefaultWebAgent.RootDomain = OAuthDomainUrl;
            WebAgent.AccessToken = accessToken;
            UserWorker userWorker = new UserWorker(this);
            userWorker.InitOrUpdateUser();
        }
        /// <summary>
        /// Creates a Reddit instance with the given WebAgent implementation
        /// </summary>
        /// <param name="agent">Implementation of IWebAgent interface. Used to generate requests.</param>
        public Reddit(IWebAgent agent)
        {
            WebAgent = agent;
            JsonSerializerSettings = new JsonSerializerSettings
            {
                CheckAdditionalContent = false,
                DefaultValueHandling = DefaultValueHandling.Ignore
            };
            CaptchaSolver = new ConsoleCaptchaSolver();
        }
        /// <summary>
        /// Creates a Reddit instance with the given WebAgent implementation
        /// </summary>
        /// <param name="agent">Implementation of IWebAgent interface. Used to generate requests.</param>
        /// <param name="initUser">Whether to run InitOrUpdateUser, requires <paramref name="agent"/> to have credentials first.</param>
        public Reddit(IWebAgent agent, bool initUser)
        {
            WebAgent = agent;
            JsonSerializerSettings = new JsonSerializerSettings
            {
                CheckAdditionalContent = false,
                DefaultValueHandling = DefaultValueHandling.Ignore
            };
            CaptchaSolver = new ConsoleCaptchaSolver();
            if(initUser)
            {
                UserWorker userWorker = new UserWorker(this);
                userWorker.InitOrUpdateUser();
            }
        }

        /// <summary>
        /// Logs in the current Reddit instance.
        /// </summary>
        /// <param name="username">The username of the user to log on to.</param>
        /// <param name="password">The password of the user to log on to.</param>
        /// <param name="useSsl">Whether to use SSL or not. (default: true)</param>
        /// <returns></returns>
        public AuthenticatedUser LogIn(string username, string password, bool useSsl = true)
        {
            UserWorker userWorker = new UserWorker(this);
            return userWorker.LogIn(username, password, useSsl);
        }

        public RedditUser GetUser(string name)
        {
            UserWorker userWorker = new UserWorker(this);
            return userWorker.GetUser(name);
        }

        #region Obsolete Getter Methods

        [Obsolete("Use User property instead")]
        public AuthenticatedUser GetMe()
        {
            return User;
        }

        #endregion Obsolete Getter Methods

        public Subreddit GetSubreddit(string name)
        {
            SubredditWorker subredditWorker = new SubredditWorker(this);
            return subredditWorker.GetSubreddit(name);
        }

        /// <summary>
        /// Returns the subreddit. 
        /// </summary>
        /// <param name="name">The name of the subreddit</param>
        /// <returns>The Subreddit by given name</returns>
        public async Task<Subreddit> GetSubredditAsync(string name)
        {
            SubredditWorker subredditWorker = new SubredditWorker(this);
            return subredditWorker.GetSubreddit(name);
        }

        public Domain GetDomain(string domain)
        {
            if (!domain.StartsWith("http://") && !domain.StartsWith("https://"))
                domain = "http://" + domain;
            var uri = new Uri(domain);
            return new Domain(this, uri, WebAgent);
        }

        public JToken GetToken(Uri uri)
        {
            var url = uri.AbsoluteUri;

            if (url.EndsWith("/"))
                url = url.Remove(url.Length - 1);

            var request = WebAgent.CreateGet(string.Format(GetPostUrl, url));
            var response = request.GetResponse();
            var data = WebAgent.GetResponseString(response.GetResponseStream());
            var json = JToken.Parse(data);

            return json[0]["data"]["children"].First;
        }

        public Post GetPost(Uri uri)
        {
            return new Post().Init(this, GetToken(uri), WebAgent);
        }

        public void ComposePrivateMessage(string subject, string body, string to, string captchaId = "", string captchaAnswer = "")
        {
            PrivateMessageWorker privateMessageWorker = new PrivateMessageWorker(this);
            privateMessageWorker.ComposePrivateMessage(subject, body, to, captchaId, captchaAnswer);
        }

        /// <summary>
        /// Registers a new Reddit user
        /// </summary>
        /// <param name="userName">The username for the new account.</param>
        /// <param name="passwd">The password for the new account.</param>
        /// <param name="email">The optional recovery email for the new account.</param>
        /// <returns>The newly created user account</returns>
        public AuthenticatedUser RegisterAccount(string userName, string passwd, string email = "")
        {
            UserWorker userWorker = new UserWorker(this);
            return userWorker.RegisterAccount(userName, passwd, email);
        }

        public Model GetModelByFullname(string fullname)
        {
            var request = WebAgent.CreateGet(string.Format(GetModelUrl, fullname));
            var response = request.GetResponse();
            var data = WebAgent.GetResponseString(response.GetResponseStream());
            var json = JToken.Parse(data);
            return Model.Parse(this, json["data"]["children"][0], WebAgent);
        }

        public Comment GetComment(string subreddit, string name, string linkName)
        {
            CommentWorker commentWorker = new CommentWorker(this);
            return commentWorker.GetComment(subreddit, name, linkName);
        }

        public Comment GetComment(Uri uri)
        {
            CommentWorker commentWorker = new CommentWorker(this);
            return commentWorker.GetComment(uri);
        }

        public Listing<T> SearchByUrl<T>(string url) where T : Model
        {
            var urlSearchQuery = string.Format(UrlSearchPattern, url);
            return Search<T>(urlSearchQuery);
        }

        public Listing<T> Search<T>(string query, Sorting sortE = Sorting.Relevance, TimeSorting timeE = TimeSorting.All) where T : Model
        {
            string sort = sortE.ToString().ToLower();
            string time = timeE.ToString().ToLower();
            return new Listing<T>(this, string.Format(SearchUrl, query, sort, time), WebAgent);
        }

        public Listing<T> SearchByTimestamp<T>(DateTime from, DateTime to, string query = "", string subreddit = "", Sorting sortE = Sorting.Relevance, TimeSorting timeE = TimeSorting.All) where T : Thing
        {
            string sort = sortE.ToString().ToLower();
            string time = timeE.ToString().ToLower();

            var fromUnix = (from - new DateTime(1970, 1, 1, 0, 0, 0)).TotalSeconds;
            var toUnix = (to - new DateTime(1970, 1, 1, 0, 0, 0)).TotalSeconds;

            string searchQuery = "(and+timestamp:" + fromUnix + ".." + toUnix + "+'" + query + "'+" + "subreddit:'" + subreddit + "')&syntax=cloudsearch";
            return new Listing<T>(this, string.Format(SearchUrl, searchQuery, sort, time), WebAgent);
        }

        #region SubredditSearching

        /// <summary>
        /// Returns a Listing of newly created subreddits.
        /// </summary>
        /// <returns></returns>
        public Listing<Subreddit> GetNewSubreddits()
        {
            return new Listing<Subreddit>(this, NewSubredditsUrl, WebAgent);
        }

        /// <summary>
        /// Returns a Listing of the most popular subreddits.
        /// </summary>
        /// <returns></returns>
        public Listing<Subreddit> GetPopularSubreddits()
        {
            return new Listing<Subreddit>(this, PopularSubredditsUrl, WebAgent);
        }

        /// <summary>
        /// Returns a Listing of Gold-only subreddits. This endpoint will not return anyModel if the authenticated Reddit account does not currently have gold.
        /// </summary>
        /// <returns></returns>
        public Listing<Subreddit> GetGoldSubreddits()
        {
            return new Listing<Subreddit>(this, GoldSubredditsUrl, WebAgent);
        }

        /// <summary>
        /// Returns the Listing of default subreddits.
        /// </summary>
        /// <returns></returns>
        public Listing<Subreddit> GetDefaultSubreddits()
        {
            return new Listing<Subreddit>(this, DefaultSubredditsUrl, WebAgent);
        }

        /// <summary>
        /// Returns the Listing of subreddits related to a query.
        /// </summary>
        /// <returns></returns>
        public Listing<Subreddit> SearchSubreddits(string query)
        {
            return new Listing<Subreddit>(this, string.Format(SearchSubredditsUrl, query), WebAgent);
        }

        #endregion SubredditSearching

        #region Helpers

        protected async internal Task<T> GetModelAsync<T>(string url) where T : Model
        {
            var request = WebAgent.CreateGet(url);
            var response = request.GetResponse();
            var data = WebAgent.GetResponseString(response.GetResponseStream());
            var json = JToken.Parse(data);
            var ret = await Model.ParseAsync(this, json, WebAgent);
            return (T)ret;
        }

        protected internal T GetModel<T>(string url) where T : Model
        {
            var request = WebAgent.CreateGet(url);
            var response = request.GetResponse();
            var data = WebAgent.GetResponseString(response.GetResponseStream());
            var json = JToken.Parse(data);
            return (T)Model.Parse(this, json, WebAgent);
        }

        #endregion
    }
}
