namespace RedditSharp
{
    public class TextData : SubmitData
    {
        [RedditAPIName("text")]
        public string Text { get; set; }

        internal TextData()
        {
            Kind = "self";
        }
    }
}
