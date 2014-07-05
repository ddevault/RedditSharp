using RedditSharp.CaptchaHandling;

namespace RedditSharp.Contracts
{
    public interface ICaptchaSolver
    {
        CaptchaResponse HandleCaptcha(Captcha captcha);
    }
}
