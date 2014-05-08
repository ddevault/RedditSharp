using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace RedditSharp
{
    /// <summary>
    /// Exception that gets thrown if you try and submit a duplicate link to a SubReddit
    /// </summary>
    public class DuplicateLinkException : RedditException
    {
        public DuplicateLinkException()
        {
        }

        public DuplicateLinkException(string message)
            : base(message)
        {
        }

        public DuplicateLinkException(string message, Exception inner)
            : base(message, inner)
        {
        }
    }
}
