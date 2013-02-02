using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace RedditSharp
{
    public class RedditException
    {
        public RedditException(string message)
        {
            Message = message;
        }

        public string Message { get; set; }
    }
}
