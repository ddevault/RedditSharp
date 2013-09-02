namespace RedditSharp
{
    public class RedditException
    {
        public RedditException(string message)
        {
            Message = message;
        }

        public string Message { get; set; }
    }
}
