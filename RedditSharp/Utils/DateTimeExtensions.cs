using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RedditSharp.Utils
{
    internal static class DateTimeExtensions
    {
        public static double DateTimeToUnixTimestamp(this DateTime dateTime)
        {
            double time = (dateTime - new DateTime(1970, 1, 1).ToLocalTime()).TotalSeconds;

            return Convert.ToInt32(time);
        }
    }
}