namespace RedditSharp
{
    internal class TextData : SubmitData
    {
        [RedditAPIName("text")]
        internal string Text { get; set; }

        internal TextData()
        {
            Kind = "self";
        }
    }
}
