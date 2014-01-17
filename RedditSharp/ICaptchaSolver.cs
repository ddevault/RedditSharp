namespace RedditSharp
{
    public interface ICaptchaSolver
    {
        CaptchaResponse HandleCaptcha(Captcha captcha);
    }
}
