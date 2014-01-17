namespace RedditSharp
{
    public interface ICaptchaSolver
    {
        string GetAnswer(string id);
    }
}
