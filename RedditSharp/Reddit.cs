using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.Linq;
using System.Net;
using System.Security.Authentication;

namespace RedditSharp
{
    public partial class Reddit
    {
        #region Constant Urls

        private const string SslLoginUrl = "https://ssl.reddit.com/api/login";
        private const string LoginUrl = "/api/login/username";
        private const string UserInfoUrl = "/user/{0}/about.json";
        private const string MeUrl = "/api/me.json";
        private const string SubredditAboutUrl = "/r/{0}/about.json";
        private const string ComposeMessageUrl = "/api/compose";
        private const string RegisterAccountUrl = "/api/register";
        private const string GetThingUrl = "/by_id/{0}.json";
        private const string GetCommentUrl = "/r/{0}/comments/{1}/foo/{2}.json";
        private const string GetPostUrl = "{0}.json";
        private const string DomainUrl = "www.reddit.com";

        #endregion

        #region Static Variables

        static Reddit()
        {
            WebAgent.UserAgent = "";
            WebAgent.EnableRateLimit = true;
            WebAgent.RootDomain = "www.reddit.com";
        }

        #endregion


        private readonly IWebAgent _webAgent;

        /// <summary>
        /// The authenticated user for this instance.
        /// </summary>
        public AuthenticatedUser User { get; set; }

        internal JsonSerializerSettings JsonSerializerSettings { get; set; }

        public Reddit()
        {
            JsonSerializerSettings = new JsonSerializerSettings();
            JsonSerializerSettings.CheckAdditionalContent = false;
            JsonSerializerSettings.DefaultValueHandling = DefaultValueHandling.Ignore;
            _webAgent = new WebAgent();
        }

        public AuthenticatedUser LogIn(string username, string password, bool useSsl = true)
        {
            if (Type.GetType("Mono.Runtime") != null)
                ServicePointManager.ServerCertificateValidationCallback = (s, c, ch, ssl) => true;
            _webAgent.Cookies = new CookieContainer();
            HttpWebRequest request;
            if (useSsl)
                request = _webAgent.CreatePost(SslLoginUrl, false);
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
            GetMe();
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

        public AuthenticatedUser GetMe()
        {
            var request = _webAgent.CreateGet(MeUrl);
            var response = (HttpWebResponse)request.GetResponse();
            var result = _webAgent.GetResponseString(response.GetResponseStream());
            var json = JObject.Parse(result);
            User = new AuthenticatedUser(this, json, _webAgent);
            return User;
        }

        public Subreddit GetSubreddit(string name)
        {
            if (name.StartsWith("r/"))
                name = name.Substring(2);
            if (name.StartsWith("/r/"))
                name = name.Substring(3);
            return (Subreddit)GetThing(string.Format(SubredditAboutUrl, name));
        }

        public JToken GetToken(string url)
        {
            if (url.EndsWith("/"))
                url = url.Remove(url.Length - 1);

            var prependDomain = !url.Contains(DomainUrl);

            var request = _webAgent.CreateGet(string.Format(GetPostUrl, url), prependDomain);
            var response = request.GetResponse();
            var data = _webAgent.GetResponseString(response.GetResponseStream());
            var json = JToken.Parse(data);

            return json[0]["data"]["children"].First;
        }
        public Post GetPost(string url)
        {
            return new Post(this, this.GetToken(url), _webAgent);
        }

        public void ComposePrivateMessage(string subject, string body, string to)
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
                uh = User.Modhash
            });
            var response = request.GetResponse();
            var result = _webAgent.GetResponseString(response.GetResponseStream());
            // TODO: Error
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
            var request = _webAgent.CreateGet(string.Format(GetThingUrl, fullname), true);
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
                var request = _webAgent.CreateGet(string.Format(GetCommentUrl, subreddit, linkName, name), true);
                var response = request.GetResponse();
                var data = _webAgent.GetResponseString(response.GetResponseStream());
                var json = JToken.Parse(data);
                return Thing.Parse(this, json[1]["data"]["children"][0], _webAgent) as Comment;
            }
            catch (WebException e)
            {
                return null;
            }
        }

        #region Helpers

        protected internal Thing GetThing(string url, bool prependDomain = true)
        {
            var request = _webAgent.CreateGet(url, prependDomain);
            var response = request.GetResponse();
            var data = _webAgent.GetResponseString(response.GetResponseStream());
            var json = JToken.Parse(data);
            return Thing.Parse(this, json, _webAgent);
        }

        #endregion
    }
}
