using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace RedditSharp
{
    [AttributeUsage(AttributeTargets.Field | AttributeTargets.Property)]
    internal class RedditAPINameAttribute : Attribute
    {
        internal string Name { get; private set; }

        internal RedditAPINameAttribute(string name)
        {
            Name = name;
        }

        public override string ToString()
        {
            return Name;
        }
    }
}
