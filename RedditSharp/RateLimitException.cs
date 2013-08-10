using System;

namespace RedditSharp
{
    public class RateLimitException : Exception
    {
        public TimeSpan TimeToReset { get; set; }

        public RateLimitException(TimeSpan timeToReset)
        {
            TimeToReset = timeToReset;
        }
    }
}

