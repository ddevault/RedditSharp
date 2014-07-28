using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.Linq;
using System.Net;
using System.Security.Authentication;
using RedditSharp.Things;

namespace RedditSharp
{
    /// <summary>
    /// Provides means of communication with the Reddit.com service.
    /// </summary>
    public class Reddit
    {
        #region Constant Urls

        private const string SslLoginUrl = "https://ssl.reddit.com/api/login";
        private const string LoginUrl = "/api/login/username";
        private const string UserInfoUrl = "/user/{0}/about.json";
        private const string MeUrl = "/api/me.json";
        private const string OAuthMeUrl = "/api/v1/me.json";
        private const string SubredditAboutUrl = "/r/{0}/about.json";
        private const string ComposeMessageUrl = "/api/compose";
        private const string RegisterAccountUrl = "/api/register";
        private const string GetThingUrl = "/api/info.json?id={0}";
        private const string GetCommentUrl = "/r/{0}/comments/{1}/foo/{2}.json";
        private const string GetPostUrl = "{0}.json";
        private const string DomainUrl = "www.reddit.com";
        private const string OAuthDomainUrl = "oauth.reddit.com";

        #endregion

        #region Static Variables

        static Reddit()
        {
            WebAgent.UserAgent = "";
            WebAgent.RateLimit = WebAgent.RateLimitMode.Pace;
            WebAgent.Protocol = "http";
            WebAgent.RootDomain = "www.reddit.com";
        }

        #endregion

        internal readonly IWebAgent _webAgent;

        /// <summary>
        /// Captcha solver instance to use when solving captchas.
        /// </summary>
        public ICaptchaSolver CaptchaSolver;

        /// <summary>
        /// The authenticated user for this instance.
        /// </summary>
        public AuthenticatedUser User { get; set; }

        /// <summary>
        /// Sets the Rate Limiting Mode of the underlying WebAgent.
        /// </summary>
        public WebAgent.RateLimitMode RateLimit
        {
            get { return WebAgent.RateLimit; }
            set { WebAgent.RateLimit = value; }
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

        /// <summary>
        /// Initializes a new instance of the RedditSharp.Reddit class.
        /// </summary>
        public Reddit()
        {
            JsonSerializerSettings = new JsonSerializerSettings
            {
                CheckAdditionalContent = false,
                DefaultValueHandling = DefaultValueHandling.Ignore
            };
            _webAgent = new WebAgent();
            CaptchaSolver = new ConsoleCaptchaSolver();
        }

        /// <summary>
        /// Initializes a new instance of the RedditSharp.Reddit class.
        /// </summary>
        /// <param name="limitMode">The Rate Limiting mode of the underlying WebAgent.</param>
        public Reddit(WebAgent.RateLimitMode limitMode)
        {
            WebAgent.UserAgent = "";
            WebAgent.RateLimit = limitMode;
            WebAgent.RootDomain = "www.reddit.com";
        }

        /// <summary>
        /// Initializes a new instance of the RedditSharp.Reddit class with the given credentials, and then logs in.
        /// </summary>
        /// <param name="username">The username of the Reddit account to log in with.</param>
        /// <param name="password">The password of the Reddit account to log in with.</param>
        /// <param name="useSsl">Whether to use Reddit's SSL security for communications.</param>
        public Reddit(string username, string password, bool useSsl = true)
            : this()
        {
            LogIn(username, password, useSsl);
        }

        /// <summary>
        /// Initializes a new instance of the RedditSharp.Reddit class with the given pre-obtained access token.
        /// </summary>
        /// <param name="accessToken">The access token with which underlying WebAgent can communicate with Reddit.</param>
        public Reddit(string accessToken)
            : this()
        {
            WebAgent.Protocol = "https";
            WebAgent.RootDomain = OAuthDomainUrl;
            _webAgent.AccessToken = accessToken;
            InitOrUpdateUser();
        }

        /// <summary>
        /// Logs in the current Reddit instance.
        /// </summary>
        /// <param name="username">The username of the user to log on to.</param>
        /// <param name="password">The password of the user to log on to.</param>
        /// <param name="useSsl">Whether to use SSL or not. (default: true)</param>
        /// <returns>An instance of the RedditSharp.Things.AuthenticatedUser class representing the logged-in user.</returns>
        public AuthenticatedUser LogIn(string username, string password, bool useSsl = true)
        {
            if (Type.GetType("Mono.Runtime") != null)
                ServicePointManager.ServerCertificateValidationCallback = (s, c, ch, ssl) => true;
            _webAgent.Cookies = new CookieContainer();
            HttpWebRequest request;
            if (useSsl)
                request = _webAgent.CreatePost(SslLoginUrl);
            else
                request = _webAgent.CreatePost(LoginUrl);
            var stream = request.GetRequestStream();
            if (useSsl)
            {
                _webAgent.WritePostBody(stream, new
                {
                    user = username,
                    passwd = password,
                    api_type = "json"
                });
            }
            else
            {
                _webAgent.WritePostBody(stream, new
                {
                    user = username,
                    passwd = password,
                    api_type = "json",
                    op = "login"
                });
            }
            stream.Close();
            var response = (HttpWebResponse)request.GetResponse();
            var result = _webAgent.GetResponseString(response.GetResponseStream());
            var json = JObject.Parse(result)["json"];
            if (json["errors"].Count() != 0)
                throw new AuthenticationException("Incorrect login.");

            InitOrUpdateUser();

            return User;
        }

        /// <summary>
        /// Gets an instance of the RedditSharp.Things.RedditUser class representing the Reddit user with the given name.
        /// </summary>
        /// <param name="name">The username of the Reddit user to obtain information about.</param>
        /// <returns>An instance of the RedditSharp.Things.RedditUser class representing the Reddit user with the name given in the <paramref name="name"/> parameter.</returns>
        public RedditUser GetUser(string name)
        {
            var request = _webAgent.CreateGet(string.Format(UserInfoUrl, name));
            var response = request.GetResponse();
            var result = _webAgent.GetResponseString(response.GetResponseStream());
            var json = JObject.Parse(result);
            return new RedditUser().Init(this, json, _webAgent);
        }

        /// <summary>
        /// Initializes the User property if it's null,
        /// otherwise replaces the existing user object
        /// with a new one fetched from reddit servers.
        /// </summary>
        public void InitOrUpdateUser()
        {
            var request = _webAgent.CreateGet(string.IsNullOrEmpty(_webAgent.AccessToken) ? MeUrl : OAuthMeUrl);
            var response = (HttpWebResponse)request.GetResponse();
            var result = _webAgent.GetResponseString(response.GetResponseStream());
            var json = JObject.Parse(result);
            User = new AuthenticatedUser().Init(this, json, _webAgent);
        }

        #region Obsolete Getter Methods

        [Obsolete("Use User property instead")]
        public AuthenticatedUser GetMe()
        {
            return User;
        }

        #endregion Obsolete Getter Methods

        /// <summary>
        /// Gets an instance of a RedditSharp.Things.Subreddit class representing the subreddit with the given name.
        /// </summary>
        /// <param name="name">The name of the Subreddit to obtain information about.</param>
        /// <returns>An instance of the RedditSharp.Things.Subreddit class representing the Subreddit with the name given in the <paramref name="name"/> parameter.</returns>
        public Subreddit GetSubreddit(string name)
        {
            if (name.StartsWith("r/"))
                name = name.Substring(2);
            if (name.StartsWith("/r/"))
                name = name.Substring(3);
            return GetThing<Subreddit>(string.Format(SubredditAboutUrl, name));
        }

        /// <summary>
        /// Gets an instance of the RedditSharp.Things.Domain class representing the Reddit domain with the given name.
        /// </summary>
        /// <param name="domain">The domain name to obtain information about.</param>
        /// <returns>An instance of the RedditSharp.Things.Domain class representing the Reddit domain with the domain name given in the <paramref name="domain"/> parameter.</returns>
        public Domain GetDomain(string domain)
        {
            if (!domain.StartsWith("http://") && !domain.StartsWith("https://"))
                domain = "http://" + domain;
            var uri = new Uri(domain);
            return new Domain(this, uri, _webAgent);
        }

        /// <summary>
        /// Gets a JSON token from the specified Uri.
        /// </summary>
        /// <param name="uri">The Uri from which to parse the JSON data from.</param>
        /// <returns>An instance of the JToken class representing the JSON data obtained from the Uri specified in the <paramref name="uri"/> parameter.</returns>
        public JToken GetToken(Uri uri)
        {
            var url = uri.AbsoluteUri;

            if (url.EndsWith("/"))
                url = url.Remove(url.Length - 1);

            var request = _webAgent.CreateGet(string.Format(GetPostUrl, url));
            var response = request.GetResponse();
            var data = _webAgent.GetResponseString(response.GetResponseStream());
            var json = JToken.Parse(data);

            return json[0]["data"]["children"].First;
        }

        /// <summary>
        /// Gets an instance of the RedditSharp.Things.Post class representing a post on Reddit at the Uri provided.
        /// </summary>
        /// <param name="uri">The Uri at which the Reddit post is found.</param>
        /// <returns>An instance of the RedditSharp.Things.Post class representing a post on Reddit found at the Uri specified in the <paramref name="uri"/> parameter.</returns>
        public Post GetPost(Uri uri)
        {
            return new Post().Init(this, GetToken(uri), _webAgent);
        }

        /// <summary>
        /// Composes a private message on Reddit.
        /// </summary>
        /// <param name="subject">The subject of the Private Message.</param>
        /// <param name="body">The message body of the Private Message. This can be formatted with Markdown.</param>
        /// <param name="to">The recipient of the Private Message. This can be the username of a Reddit user, or the name of a Reddit subreddit, including the <c>/r/</c> prefix, eg. <c>/r/AskReddit</c>.</param>
        /// <param name="captchaId">The ID of the Captcha associated with sending the Captcha.</param>
        /// <param name="captchaAnswer">The solved text of the Captcha associated with sending the Captcha.</param>
        public void ComposePrivateMessage(string subject, string body, string to, string captchaId = "", string captchaAnswer = "")
        {
            if (User == null)
                throw new Exception("User can not be null.");
            var request = _webAgent.CreatePost(ComposeMessageUrl);
            _webAgent.WritePostBody(request.GetRequestStream(), new
            {
                api_type = "json",
                subject,
                text = body,
                to,
                uh = User.Modhash,
                iden = captchaId,
                captcha = captchaAnswer
            });
            var response = request.GetResponse();
            var result = _webAgent.GetResponseString(response.GetResponseStream());
            var json = JObject.Parse(result);

            ICaptchaSolver solver = CaptchaSolver; // Prevent race condition

            if (json["json"]["errors"].Any() && json["json"]["errors"][0][0].ToString() == "BAD_CAPTCHA" && solver != null)
            {
                captchaId = json["json"]["captcha"].ToString();
                CaptchaResponse captchaResponse = solver.HandleCaptcha(new Captcha(captchaId));

                if (!captchaResponse.Cancel) // Keep trying until we are told to cancel
                    ComposePrivateMessage(subject, body, to, captchaId, captchaResponse.Answer);
            }
        }

        /// <summary>
        /// Registers a new Reddit user and returns an instance of the RedditSharp.Things.AuthenticatedUser class describing the new user.
        /// </summary>
        /// <param name="userName">The username for the new account.</param>
        /// <param name="passwd">The password for the new account.</param>
        /// <param name="email">The optional recovery email for the new account.</param>
        /// <returns>An instance of the RedditSharp.Things.AuthenticatedUser class describing the new user named in the <paramref name="userName"/> parameter.</returns>
        public AuthenticatedUser RegisterAccount(string userName, string passwd, string email = "")
        {
            var request = _webAgent.CreatePost(RegisterAccountUrl);
            _webAgent.WritePostBody(request.GetRequestStream(), new
            {
                api_type = "json",
                email = email,
                passwd = passwd,
                passwd2 = passwd,
                user = userName
            });
            var response = request.GetResponse();
            var result = _webAgent.GetResponseString(response.GetResponseStream());
            var json = JObject.Parse(result);
            return new AuthenticatedUser().Init(this, json, _webAgent);
            // TODO: Error
        }

        /// <summary>
        /// Gets a Reddit thing with the given full name.
        /// </summary>
        /// <param name="fullname">THe full name of the Reddit thing to obtain information about.</param>
        /// <returns>An instance of the RedditSharp.Things.Thing class describing the Reddit thing with the full name given in the <paramref name="fullname"/> parameter.</returns>
        public Thing GetThingByFullname(string fullname)
        {
            var request = _webAgent.CreateGet(string.Format(GetThingUrl, fullname));
            var response = request.GetResponse();
            var data = _webAgent.GetResponseString(response.GetResponseStream());
            var json = JToken.Parse(data);
            return Thing.Parse(this, json["data"]["children"][0], _webAgent);
        }

        /// <summary>
        /// Gets an instance of the RedditSharp.Things.Comment class describing a comment found at the given location.
        /// </summary>
        /// <param name="subreddit">The full name of the subreddit, without the <c>/r/</c> prefix.</param>
        /// <param name="name">The name of the comment thing to obtain information about.</param>
        /// <param name="linkName">The name of the post thing on which the comment is posted.</param>
        /// <returns>An instance of the RedditSharp.Things.Comment class describing a comment found 
        /// at the location specified with the <paramref name="name"/> and <paramref name="linkName"/> parameters.</returns>
        /// <remarks>Note that it isn't even necessary to provide the subreddit in the Uri. 
        /// The only change is that the subreddit style is not displayed on the resultant URL.
        /// For an example, see http://www.reddit.com/comments/27eh97/foo/ci01pzf. </remarks>
        public Comment GetComment(string subreddit, string name, string linkName)
        {
            try
            {
                if (linkName.StartsWith("t3_"))
                    linkName = linkName.Substring(3);
                if (name.StartsWith("t1_"))
                    name = name.Substring(3);
                var request = _webAgent.CreateGet(string.Format(GetCommentUrl, subreddit, linkName, name));
                var response = request.GetResponse();
                var data = _webAgent.GetResponseString(response.GetResponseStream());
                var json = JToken.Parse(data);
                return Thing.Parse(this, json[1]["data"]["children"][0], _webAgent) as Comment;
            }
            catch (WebException)
            {
                return null;
            }
        }

        #region Helpers

        protected internal T GetThing<T>(string url) where T : Thing
        {
            var request = _webAgent.CreateGet(url);
            var response = request.GetResponse();
            var data = _webAgent.GetResponseString(response.GetResponseStream());
            var json = JToken.Parse(data);
            return (T)Thing.Parse(this, json, _webAgent);
        }

        #endregion
    }
}
