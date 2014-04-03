using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace RedditSharp
{
    [AttributeUsage(AttributeTargets.Field | AttributeTargets.Property)]
    public class RedditAPINameAttribute : Attribute
    {
        public string Name { get; private set; }

        public RedditAPINameAttribute(string name)
        {
            Name = name;
        }

        public override string ToString()
        {
            return Name;
        }
    }
}
