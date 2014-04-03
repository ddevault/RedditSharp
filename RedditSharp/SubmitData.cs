namespace RedditSharp
{
    public abstract class SubmitData
    {
        [RedditAPIName("api_type")]
        public string APIType { get; set; }

        [RedditAPIName("kind")]
        public string Kind { get; set; }

        [RedditAPIName("sr")]
        public string Subreddit { get; set; }

        [RedditAPIName("uh")]
        public string UserHash { get; set; }

        [RedditAPIName("title")]
        public string Title { get; set; }

        [RedditAPIName("iden")]
        public string Iden { get; set; }

        [RedditAPIName("captcha")]
        public string Captcha { get; set; }

        protected SubmitData()
        {
            APIType = "json";
        }
    }
}
