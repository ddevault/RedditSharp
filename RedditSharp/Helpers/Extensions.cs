using System.Collections.Generic;
using Newtonsoft.Json.Linq;

namespace RedditSharp.Helpers
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
