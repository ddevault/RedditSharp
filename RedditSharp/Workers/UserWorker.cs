using Newtonsoft.Json.Linq;
using RedditSharp.Models;
using System;
using System.Linq;
using System.Net;
using System.Security.Authentication;

namespace RedditSharp.Workers
{
    class UserWorker
    {
        private const string SslLoginUrl = "https://ssl.reddit.com/api/login";
        private const string LoginUrl = "/api/login/username";
        private const string MeUrl = "/api/me.json";
        private const string OAuthMeUrl = "/api/v1/me.json";
        private const string UserInfoUrl = "/user/{0}/about.json";
        private const string RegisterAccountUrl = "/api/register";

        private Reddit reddit;

        public UserWorker(Reddit reddit)
        {
            this.reddit = reddit;
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
            reddit.WebAgent.Cookies = new CookieContainer();
            HttpWebRequest request;
            if (useSsl)
                request = reddit.WebAgent.CreatePost(SslLoginUrl);
            else
                request = reddit.WebAgent.CreatePost(LoginUrl);
            var stream = request.GetRequestStream();
            if (useSsl)
            {
                reddit.WebAgent.WritePostBody(stream, new
                {
                    user = username,
                    passwd = password,
                    api_type = "json"
                });
            }
            else
            {
                reddit.WebAgent.WritePostBody(stream, new
                {
                    user = username,
                    passwd = password,
                    api_type = "json",
                    op = "login"
                });
            }
            stream.Close();
            var response = (HttpWebResponse)request.GetResponse();
            var result = reddit.WebAgent.GetResponseString(response.GetResponseStream());
            var json = JObject.Parse(result)["json"];
            if (json["errors"].Count() != 0)
                throw new AuthenticationException("Incorrect login.");

            InitOrUpdateUser();

            return reddit.User;
        }

        public RedditUser GetUser(string name)
        {
            var request = reddit.WebAgent.CreateGet(string.Format(UserInfoUrl, name));
            var response = request.GetResponse();
            var result = reddit.WebAgent.GetResponseString(response.GetResponseStream());
            var json = JObject.Parse(result);
            return new RedditUser().Init(reddit, json, reddit.WebAgent);
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
            var request = reddit.WebAgent.CreatePost(RegisterAccountUrl);
            reddit.WebAgent.WritePostBody(request.GetRequestStream(), new
            {
                api_type = "json",
                email = email,
                passwd = passwd,
                passwd2 = passwd,
                user = userName
            });
            var response = request.GetResponse();
            var result = reddit.WebAgent.GetResponseString(response.GetResponseStream());
            var json = JObject.Parse(result);
            return new AuthenticatedUser().Init(reddit, json, reddit.WebAgent);
            // TODO: Error
        }

        /// <summary>
        /// Initializes the User property if it's null,
        /// otherwise replaces the existing user object
        /// with a new one fetched from reddit servers.
        /// </summary>
        public void InitOrUpdateUser()
        {
            var request = reddit.WebAgent.CreateGet(string.IsNullOrEmpty(reddit.WebAgent.AccessToken) ? MeUrl : OAuthMeUrl);
            var response = (HttpWebResponse)request.GetResponse();
            var result = reddit.WebAgent.GetResponseString(response.GetResponseStream());
            var json = JObject.Parse(result);
            reddit.User = new AuthenticatedUser().Init(reddit, json, reddit.WebAgent);
        }
    }
}
