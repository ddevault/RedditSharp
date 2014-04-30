using System;
using System.IO;
using System.Linq;
using System.Net;
using System.Reflection;
using System.Text;
using System.Threading;
using System.Web;

namespace RedditSharp
{
    public sealed class WebAgent : IWebAgent
    {
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
        /// It is strongly advised that you leave this set to Burst or Pace. Reddit bans excessive
        /// requests with extreme predjudice.
        /// </summary>
        public static RateLimitMode RateLimit { get; set; }

        /// <summary>
        /// The method by which the WebAgent will limit request rate
        /// </summary>
        public enum RateLimitMode
        {
            /// <summary>
            /// Limits requests to one every two seconds
            /// </summary>
            Pace,
            /// <summary>
            /// Restricts requests to thirty per minute
            /// </summary>
            Burst,
            /// <summary>
            /// Does not restrict request rate. ***NOT RECOMMENDED***
            /// </summary>
            None
        }

        /// <summary>
        /// The root domain RedditSharp uses to address Reddit.
        /// www.reddit.com by default
        /// </summary>
        public static string RootDomain { get; set; }

        /// <summary>
        /// Used to make calls against Reddit's API using OAuth23
        /// </summary>
        public string AccessToken { get; set; }

        public CookieContainer Cookies { get; set; }
        public string AuthCookie { get; set; }

        private static DateTime _lastRequest;
        private static DateTime _burstStart;
        private static int _requestsThisBurst;

        public HttpWebRequest CreateRequest(string url, string method)
        {
            var prependDomain = !Uri.IsWellFormedUriString(url, UriKind.Absolute);
            switch (RateLimit)
            {
                case RateLimitMode.Pace:
                    while ((DateTime.Now - _lastRequest).TotalSeconds < 2)// Rate limiting
                        Thread.Sleep(250);
                    _lastRequest = DateTime.Now;
                    break;
                case RateLimitMode.Burst:
                    if (_requestsThisBurst == 0)//this is first request
                        _burstStart = DateTime.Now;
                    if (_requestsThisBurst >= 30) //limit has been reached
                    {
                        while ((DateTime.UtcNow - _burstStart).TotalSeconds < 60)
                            Thread.Sleep(250);
                        _burstStart = DateTime.Now;
                    }
                    _requestsThisBurst++;
                    break;
            }
            HttpWebRequest request;
            if (prependDomain)
                request = (HttpWebRequest)WebRequest.Create(String.Format("http://{0}{1}", RootDomain, url));
            else
                request = (HttpWebRequest)WebRequest.Create(url);
            request.CookieContainer = Cookies;
            if (Type.GetType("Mono.Runtime") != null)
            {
                var cookieHeader = Cookies.GetCookieHeader(new Uri("http://reddit.com"));
                request.Headers.Set("Cookie", cookieHeader);
            }
            if (!string.IsNullOrEmpty(AccessToken))// use OAuth
            {
                request.Headers.Set("Authorization", "bearer " + AccessToken);//Must be included in OAuth calls
            }
            request.Method = method;
            request.UserAgent = UserAgent + " - with RedditSharp by /u/sircmpwn";
            return request;
        }

        public HttpWebRequest CreateGet(string url)
        {
            return CreateRequest(url, "GET");
        }

        public HttpWebRequest CreatePost(string url)
        {
            var request = CreateRequest(url, "POST");
            request.ContentType = "application/x-www-form-urlencoded";
            return request;
        }

        public string GetResponseString(Stream stream)
        {
            var data = new StreamReader(stream).ReadToEnd();
            stream.Close();
            return data;
        }

        public void WritePostBody(Stream stream, object data, params string[] additionalFields)
        {
            var type = data.GetType();
            var properties = type.GetProperties(BindingFlags.Instance | BindingFlags.NonPublic | BindingFlags.Public);
            string value = "";
            foreach (var property in properties)
            {
                var attr = property.GetCustomAttributes(typeof(RedditAPINameAttribute), false).FirstOrDefault() as RedditAPINameAttribute;
                string name = attr == null ? property.Name : attr.Name;
                var entry = Convert.ToString(property.GetValue(data, null));
                value += name + "=" + HttpUtility.UrlEncode(entry).Replace(";", "%3B").Replace("&", "%26") + "&";
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
    }
}
