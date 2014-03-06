namespace RedditSharp
{
    internal abstract class SubmitData
    {
        internal string api_type { get; set; }

        internal string kind { get; set; }

        internal string sr { get; set; }

        internal string uh { get; set; }

        internal string title { get; set; }

        internal string iden { get; set; }

        internal string captcha { get; set; }

        protected SubmitData()
        {
            api_type = "json";
        }
    }
}
