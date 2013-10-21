using Newtonsoft.Json.Linq;

namespace RedditSharp
{
    public class Thing
    {
        public static Thing Parse(Reddit reddit, JToken json, IWebAgent webAgent)
        {
            var kind = json["kind"].ValueOrDefault<string>();
            switch (kind)
            {
                case "t1":
                    return new Comment(reddit, json, webAgent);
                case "t2":
                    return new RedditUser(reddit, json, webAgent); 
                case "t3":
                    return new Post(reddit, json, webAgent);
                case "t4":
                    return new PrivateMessage(reddit, json, webAgent);
                case "t5":
                    return new Subreddit(reddit, json, webAgent);
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
