namespace RedditSharp
{
    public class LinkData : SubmitData
    {
        public string extension = "json";
        public string url;

        public LinkData()
        {
            kind = "link";
        }
    }
}
