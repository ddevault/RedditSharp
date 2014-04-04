namespace RedditSharp
{
    public class LinkData : SubmitData
    {
        [RedditAPIName("extension")]
        public string Extension { get; set; }
        
        [RedditAPIName("url")]
        public string URL { get; set; }

        internal LinkData()
        {
            Extension = "json";
            Kind = "link";
        }
    }
}
