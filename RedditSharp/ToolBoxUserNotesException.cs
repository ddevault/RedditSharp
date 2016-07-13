using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RedditSharp
{
    class ToolBoxUserNotesException : Exception
    {
        public ToolBoxUserNotesException()
        {
        }

        public ToolBoxUserNotesException(string message)
            : base(message)
        {
        }

        public ToolBoxUserNotesException(string message, Exception inner)
            : base(message, inner)
        {
        }
    }
}
