namespace RedditSharp
{
    public struct Captcha
    {
        private const string UrlFormat = "http://www.reddit.com/captcha/{0}";

        public readonly string Id;
        public readonly string Url;

        internal Captcha(string id)
        {
            Id = id;
            Url = string.Format(UrlFormat, Id);
        }
    }
}
