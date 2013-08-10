using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Security.Authentication;
using System.Text;
using System.Net;
using Newtonsoft.Json.Linq;
using System.Web;
using Newtonsoft.Json;

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

        #endregion

        #region Static Variables

        static Reddit()
        {
            UserAgent = "";
            EnableRateLimit = true;
            RootDomain = "www.reddit.com";
        }

        /// <summary>
        /// Additional values to append to the default RedditSharp user agent.
        /// </summary>
        public static string UserAgent { get; set; }
        /// <summary>
        /// It is strongly advised that you leave this enabled. Reddit bans excessive
        /// requests with extreme predjudice.
        /// </summary>
        public static bool EnableRateLimit { get; set; }
        /// <summary>
        /// The root domain RedditSharp uses to address Reddit.
        /// www.reddit.com by default
        /// </summary>
        public static string RootDomain { get; set; }

        #endregion

        /// <summary>
        /// The authenticated user for this instance.
        /// </summary>
        public AuthenticatedUser User { get; set; }

        internal JsonSerializerSettings JsonSerializerSettings { get; set; }

        private CookieContainer Cookies { get; set; }
        private string AuthCookie { get; set; }

        public Reddit()
        {
            JsonSerializerSettings = new JsonSerializerSettings();
            JsonSerializerSettings.CheckAdditionalContent = false;
            JsonSerializerSettings.DefaultValueHandling = DefaultValueHandling.Ignore;
        }

        public AuthenticatedUser LogIn(string username, string password, bool useSsl = true)
        {
            if (Type.GetType("Mono.Runtime") != null)
                ServicePointManager.ServerCertificateValidationCallback = (s, c, ch, ssl) => true;
            Cookies = new CookieContainer();
            HttpWebRequest request;
            if (useSsl)
                request = CreatePost(SslLoginUrl, false);
            else
                request = CreatePost(LoginUrl);
            var stream = request.GetRequestStream();
            if (useSsl)
            {
                WritePostBody(stream, new
                {
                    user = username,
                    passwd = password,
                    api_type = "json"
                });
            }
            else
            {
                WritePostBody(stream, new
                {
                    user = username,
                    passwd = password,
                    api_type = "json",
                    op = "login"
                });
            }
            stream.Close();
            var response = (HttpWebResponse)request.GetResponse();
            var result = GetResponseString(response.GetResponseStream());
            var json = JObject.Parse(result)["json"];
            if (json["errors"].Count() != 0)
                throw new AuthenticationException("Incorrect login.");
            GetMe();
            return User;
        }

        public RedditUser GetUser(string name)
        {
            var request = CreateGet(string.Format(UserInfoUrl, name));
            var response = request.GetResponse();
            var result = GetResponseString(response.GetResponseStream());
            var json = JObject.Parse(result);
            return new RedditUser(this, json);
        }

        public AuthenticatedUser GetMe()
        {
            var request = CreateGet(MeUrl);
            var response = (HttpWebResponse)request.GetResponse();
            var result = GetResponseString(response.GetResponseStream());
            var json = JObject.Parse(result);
            User = new AuthenticatedUser(this, json);
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

        public void ComposePrivateMessage(string subject, string body, string to)
        {
            if (User == null)
                throw new Exception("User can not be null.");
            var request = CreatePost(ComposeMessageUrl);
            WritePostBody(request.GetRequestStream(), new
            {
                api_type = "json",
                subject,
                text = body,
                to,
                uh = User.Modhash
            });
            var response = request.GetResponse();
            var result = GetResponseString(response.GetResponseStream());
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
            var request = CreatePost(RegisterAccountUrl);
            WritePostBody(request.GetRequestStream(), new
            {
                api_type = "json",
                email = email,
                passwd = passwd,
                passwd2 = passwd,
                user = userName
            });
            var response = request.GetResponse();
            var result = GetResponseString(response.GetResponseStream());
            var json = JObject.Parse(result);
            return new AuthenticatedUser(this, json);
            // TODO: Error
        }

        #region Helpers

        private static DateTime lastRequest = DateTime.MinValue;
        protected internal HttpWebRequest CreateRequest(string url, string method, bool prependDomain = true)
        {
            while (EnableRateLimit && (DateTime.Now - lastRequest).TotalSeconds < 2) ; // Rate limiting
            lastRequest = DateTime.Now;
            HttpWebRequest request;
            if (prependDomain)
                request = (HttpWebRequest)WebRequest.Create(string.Format("http://{0}{1}", RootDomain, url));
            else
                request = (HttpWebRequest)WebRequest.Create(url);
            request.CookieContainer = Cookies;
            if (Type.GetType("Mono.Runtime") != null)
            {
                var cookieHeader = Cookies.GetCookieHeader(new Uri("http://reddit.com"));
                request.Headers.Set("Cookie", cookieHeader);
            }
            request.Method = method;
            request.UserAgent = UserAgent + " - with RedditSharp by /u/sircmpwn";
            return request;
        }

        protected internal HttpWebRequest CreateGet(string url, bool prependDomain = true)
        {
            return CreateRequest(url, "GET", prependDomain);
        }

        protected internal HttpWebRequest CreatePost(string url, bool prependDomain = true)
        {
            var request = CreateRequest(url, "POST", prependDomain);
            request.ContentType = "application/x-www-form-urlencoded";
            return request;
        }

        protected internal string GetResponseString(Stream stream)
        {
            var data = new StreamReader(stream).ReadToEnd();
            stream.Close();
            return data;
        }

        protected internal Thing GetThing(string url, bool prependDomain = true)
        {
            var request = CreateGet(url, prependDomain);
            var response = request.GetResponse();
            var data = GetResponseString(response.GetResponseStream());
            var json = JToken.Parse(data);
            return Thing.Parse(this, json);
        }

        protected internal void WritePostBody(Stream stream, object data, params string[] additionalFields)
        {
            var type = data.GetType();
            var properties = type.GetProperties();
            string value = "";
            foreach (var property in properties)
            {
                var entry = Convert.ToString(property.GetValue(data, null));
                value += property.Name + "=" + HttpUtility.UrlEncode(entry).Replace(";", "%3B").Replace("&", "%26") + "&";
            }
            for (int i = 0; i < additionalFields.Length; i += 2)
            {
                var entry = Convert.ToString(additionalFields[i + 1]);
                if (entry == null)
                    entry = string.Empty;
                value += additionalFields[i] + "=" + HttpUtility.UrlEncode(entry).Replace(";", "%3B").Replace("&", "%26") + "&";
            }
            value = value.Remove(value.Length - 1); // Remove trailing &
            var raw = Encoding.UTF8.GetBytes(value);
            stream.Write(raw, 0, raw.Length);
            stream.Close();
        }

        protected internal static DateTime UnixTimeStampToDateTime(double unixTimeStamp)
        {
            // Unix timestamp is seconds past epoch
            var dtDateTime = new DateTime(1970, 1, 1, 0, 0, 0, 0);
            dtDateTime = dtDateTime.AddSeconds(unixTimeStamp).ToLocalTime();
            return dtDateTime;
        }

        #endregion
    }
}
