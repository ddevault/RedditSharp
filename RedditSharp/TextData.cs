namespace RedditSharp
{
    internal class TextData : SubmitData
    {
        internal string text;

        internal TextData()
        {
            kind = "self";
        }
    }
}
