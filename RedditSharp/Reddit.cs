using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Security.Authentication;
using System.Text;
using System.Net;
using Newtonsoft.Json.Linq;

namespace RedditSharp
{
    public partial class Reddit
    {
        #region Constant Urls

        private const string LoginUrl = "https://ssl.reddit.com/api/login";
        private const string UserInfoUrl = "http://www.reddit.com/user/{0}/about.json";
        private const string MeUrl = "http://www.reddit.com/api/me.json";
        private const string SubredditAboutUrl = "http://www.reddit.com/r/{0}/about.json"; 

        #endregion

        #region Static Variables

        static Reddit()
        {
            UserAgent = "";
            EnableRateLimit = true;
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

        #endregion

        /// <summary>
        /// The authenticated user for this instance.
        /// </summary>
        public AuthenticatedUser User { get; set; }

        private CookieContainer Cookies { get; set; }
        private string AuthCookie { get; set; }

        public AuthenticatedUser LogIn(string username, string password)
        {
            ServicePointManager.ServerCertificateValidationCallback = (s, c, ch, ssl) => true;
            Cookies = new CookieContainer();
            var request = CreatePost(LoginUrl);
            var stream = request.GetRequestStream();
            WritePostBody(stream, new
                {
                    user = username,
                    passwd = password
                });
            stream.Close();
            var response = request.GetResponse();
            var result = GetResponseString(response.GetResponseStream());
            var json = JObject.Parse(result);
            if (json["jquery"].Count(i => i[0].Value<int>() == 10 && i[1].Value<int>() == 11) != 0)
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
            var request = CreateGet(string.Format(SubredditAboutUrl, name));
            var response = request.GetResponse();
            var result = GetResponseString(response.GetResponseStream());
            var json = JObject.Parse(result);
            return new Subreddit(this, json);
        }

        #region Helpers

        private static DateTime lastRequest = DateTime.MinValue;
        protected internal HttpWebRequest CreateRequest(string url, string method)
        {
            while (EnableRateLimit && (DateTime.Now - lastRequest).TotalSeconds < 2) ; // Rate limiting
            lastRequest = DateTime.Now;
            var request = (HttpWebRequest)WebRequest.Create(url);
            var cookieHeader = Cookies.GetCookieHeader(new Uri("http://reddit.com"));
            request.Headers.Set("Cookie", cookieHeader);
            request.Method = method;
            request.UserAgent = UserAgent + " - with RedditSharp by /u/sircmpwn";
            return request;
        }

        protected internal HttpWebRequest CreateGet(string url)
        {
            return CreateRequest(url, "GET");
        }

        protected internal HttpWebRequest CreatePost(string url)
        {
            var request = CreateRequest(url, "POST");
            request.ContentType = "application/x-www-form-urlencoded";
            return request;
        }

        protected internal string GetResponseString(Stream stream)
        {
            var data = new StreamReader(stream).ReadToEnd();
            stream.Close();
            return data;
        }

        protected internal void WritePostBody(Stream stream, object data)
        {
            var type = data.GetType();
            var properties = type.GetProperties();
            string value = "";
            foreach (var property in properties)
            {
                var entry = Convert.ToString(property.GetValue(data, null));
                value += property.Name + "=" + Uri.EscapeDataString(entry) + "&";
            }
            value = value.Remove(value.Length - 1); // Remove trailing &
            var raw = Encoding.UTF8.GetBytes(value);
            stream.Write(raw, 0, raw.Length);
        }

        public static DateTime UnixTimeStampToDateTime(double unixTimeStamp)
        {
            // Unix timestamp is seconds past epoch
            var dtDateTime = new DateTime(1970, 1, 1, 0, 0, 0, 0);
            dtDateTime = dtDateTime.AddSeconds(unixTimeStamp).ToLocalTime();
            return dtDateTime;
        }

        #endregion
    }
}
