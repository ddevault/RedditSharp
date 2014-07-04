using System;

namespace RedditSharp
{
    public struct Captcha
    {
        private const string UrlFormat = "http://www.reddit.com/captcha/{0}";

        public readonly string Id;
        public readonly Uri Url;

        internal Captcha(string id)
        {
            Id = id;
            Url = new Uri(string.Format(UrlFormat, Id), UriKind.Absolute);
        }
    }
}
