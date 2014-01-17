namespace RedditSharp
{
    public abstract class CaptchaSolver
    {
        public abstract CaptchaResponse HandleCaptcha(Captcha captcha);
    }
}
