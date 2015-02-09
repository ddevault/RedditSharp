using System;

namespace RedditSharp
{
    public static class DateTimeExtensions
    {
        public static long ToEpoch(this DateTime date)
        {
            return (long)(date - new DateTime(1970, 1, 1)).TotalSeconds;
        }
    }
}