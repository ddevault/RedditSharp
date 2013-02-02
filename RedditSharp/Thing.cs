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
            if (data["name"] != null)
                FullName = data["name"].Value<string>();
            Id = data["id"].Value<string>();
            if (data["kind"] != null)
                Kind = data["kind"].Value<string>();
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
