using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace RedditSharp
{
    public static class UnixTimeStamp
    {
        public static DateTime UnixTimeStampToDateTime(this double unixTimeStamp)
        {
            // Unix timestamp is seconds past epoch
            var dtDateTime = new DateTime(1970, 1, 1, 0, 0, 0, 0);
            dtDateTime = dtDateTime.AddSeconds(unixTimeStamp).ToLocalTime();
            return dtDateTime;
        }
    }
}
