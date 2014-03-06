namespace RedditSharp
{
    internal class LinkData : SubmitData
    {
        internal string extension { get; set; }
        internal string url { get; set; }

        internal LinkData()
        {
            extension = "json";
            kind = "link";
        }
    }
}
