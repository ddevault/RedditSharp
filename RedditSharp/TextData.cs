namespace RedditSharp
{
    internal class TextData : SubmitData
    {
        internal string text { get; set; }

        internal TextData()
        {
            kind = "self";
        }
    }
}
