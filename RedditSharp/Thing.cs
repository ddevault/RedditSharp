using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Newtonsoft.Json.Linq;
using Newtonsoft.Json;

namespace RedditSharp
{
    public class Thing
    {
        public static Thing Parse(Reddit reddit, JToken json)
        {
            var kind = json["kind"].ValueOrDefault<string>();
            switch (kind)
            {
                case "t1":
                    return new Comment(reddit, json);
                case "t2":
                    return new RedditUser(reddit, json);
                case "t3":
                    return new Post(reddit, json);
                case "t4": // TODO: Private Message
                    return null;
                case "t5":
                    return new Subreddit(reddit, json);
                default:
                    return null;
            }
        }

        internal Thing(JToken json)
        {
            if (json == null)
                return;
            var data = json["data"];
            FullName = data["name"].ValueOrDefault<string>();
            Id = data["id"].ValueOrDefault<string>();
            Kind = json["kind"].ValueOrDefault<string>();
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
