namespace RedditSharp
{
    public class CaptchaResponse
    {
        public readonly string Answer;

        public bool Cancel { get { return string.IsNullOrEmpty(Answer); } }

        public CaptchaResponse(string answer = null)
        {
            Answer = answer;
        }
    }
}
