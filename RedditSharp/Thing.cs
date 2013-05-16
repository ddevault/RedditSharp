using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Newtonsoft.Json.Linq;

namespace RedditSharp
{
    public class Thing
    {
        public Thing(JToken json)
        {
            if (json == null)
                return;
            var data = json["data"];
            FullName = data["name"].ValueOrDefault<string>();
            Id = data["id"].ValueOrDefault<string>();
            Kind = data["kind"].ValueOrDefault<string>();
        }

        public string Shortlink
        {
            get { return "http://redd.it/" + Id; }
        }
        
        public string Id { get; set; }
        public string FullName { get; set; }
        public string Kind { get; set; }
    }
}
