using System;
using RedditSharp.Exceptions;

namespace RedditSharp.CaptchaHandling
{
    public class CaptchaFailedException : RedditException
    {
        public CaptchaFailedException()
        {
            
        }

        public CaptchaFailedException(string message)
            : base(message)
        {
            
        }

        public CaptchaFailedException(string message, Exception inner)
            : base(message, inner)
        {
            
        }
    }
}
