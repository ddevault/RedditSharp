using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Newtonsoft.Json.Linq;

namespace RedditSharp
{
    public class CreatedThing : Thing
    {
        private Reddit Reddit { get; set; }

        public CreatedThing(Reddit reddit, JToken json) : base(json)
        {
            Reddit = reddit;

            var data = json["data"];
            Created = Reddit.UnixTimeStampToDateTime(data["created"].ValueOrDefault<double>());
        }

        public DateTime Created { get; set; }
    }
}
