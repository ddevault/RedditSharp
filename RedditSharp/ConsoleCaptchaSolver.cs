using System;

namespace RedditSharp
{
    public class ConsoleCaptchaSolver : ICaptchaSolver
    {
        public CaptchaResponse HandleCaptcha(Captcha captcha)
        {
            Console.WriteLine("Captcha required! The captcha ID is {0}", captcha.Id);
            Console.WriteLine("You can find the captcha image at this url: {0}", captcha.Url);
            Console.WriteLine("Please input your captcha response or empty string to cancel:");
            var response = Console.ReadLine();
            CaptchaResponse captchaResponse = new CaptchaResponse(string.IsNullOrEmpty(response) ? null : response);
            return captchaResponse;
        }
    }
}
