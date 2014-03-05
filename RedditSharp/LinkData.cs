namespace RedditSharp
{
    internal class LinkData : SubmitData
    {
        internal string extension = "json";
        internal string url;

        internal LinkData()
        {
            kind = "link";
        }
    }
}
