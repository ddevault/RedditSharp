namespace RedditSharp
{
    public class TextData : SubmitData
    {
        public string text;

        public TextData()
        {
            kind = "self";
        }
    }
}
