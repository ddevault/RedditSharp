using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace RedditSharp
{
    public static class Extensions
    {
        public static T ValueOrDefault<T>(this IEnumerable<JToken> enumerable)
        {
            if (enumerable == null)
                return default(T);
            return enumerable.Value<T>();
        }
    }
}
