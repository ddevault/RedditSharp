using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.Linq;
using System.Net;
using System.Security.Authentication;

namespace RedditSharp
{
    /// <summary>
    /// Class to communicate with Reddit.com
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
        private const string GetThingUrl = "/by_id/{0}.json";
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
        /// Sets the Rate Limiting Mode of the underlying WebAgent
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

        public Reddit(WebAgent.RateLimitMode limitMode)
        {
            WebAgent.UserAgent = "";
            WebAgent.RateLimit = limitMode;
            WebAgent.RootDomain = "www.reddit.com";
        }

        public Reddit(string username, string password, bool useSsl = true)
            : this()
        {
            LogIn(username, password, useSsl);
        }

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
        /// <returns></returns>
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

        public RedditUser GetUser(string name)
        {
            var request = _webAgent.CreateGet(string.Format(UserInfoUrl, name));
            var response = request.GetResponse();
            var result = _webAgent.GetResponseString(response.GetResponseStream());
            var json = JObject.Parse(result);
            return new RedditUser(this, json, _webAgent);
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
            User = new AuthenticatedUser(this, json, _webAgent);
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
            if (name.StartsWith("r/"))
                name = name.Substring(2);
            if (name.StartsWith("/r/"))
                name = name.Substring(3);
            return GetThing<Subreddit>(string.Format(SubredditAboutUrl, name));
        }

        public Domain GetDomain(string domain)
        {
            if (!domain.StartsWith("http://") && !domain.StartsWith("https://"))
                domain = "http://" + domain;
            var uri = new Uri(domain);
            return new Domain(this, uri, _webAgent);
        }

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

        public Post GetPost(Uri uri)
        {
            return new Post(this, GetToken(uri), _webAgent);
        }

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
        /// Registers a new Reddit user
        /// </summary>
        /// <param name="userName">The username for the new account.</param>
        /// <param name="passwd">The password for the new account.</param>
        /// <param name="email">The optional recovery email for the new account.</param>
        /// <returns>The newly created user account</returns>
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
            return new AuthenticatedUser(this, json, _webAgent);
            // TODO: Error
        }

        public Thing GetThingByFullname(string fullname)
        {
            var request = _webAgent.CreateGet(string.Format(GetThingUrl, fullname));
            var response = request.GetResponse();
            var data = _webAgent.GetResponseString(response.GetResponseStream());
            var json = JToken.Parse(data);
            return Thing.Parse(this, json["data"]["children"][0], _webAgent);
        }

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
