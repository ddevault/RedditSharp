namespace RedditSharp
{
    public class TextData : SubmitData
    {
        [RedditAPIName("text")]
        public string Text { get; set; }

        public TextData()
        {
            Kind = "self";
        }
    }
}
