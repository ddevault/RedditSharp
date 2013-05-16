using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Newtonsoft.Json.Linq;

namespace RedditSharp
{
    public class RedditUser
    {
        public RedditUser(Reddit reddit, JToken json)
        {
            var data = json["data"];
            Name = data["name"].ValueOrDefault<string>();
            Id = data["id"].ValueOrDefault<string>();
            HasGold = data["is_gold"].ValueOrDefault<bool>();
            IsModerator = data["is_mod"].ValueOrDefault<bool>();
            LinkKarma = data["link_karma"].ValueOrDefault<int>();
            CommentKarma = data["comment_karma"].ValueOrDefault<int>();
            Created = Reddit.UnixTimeStampToDateTime(data["created"].ValueOrDefault<double>());
            Reddit = reddit;
        }

        protected Reddit Reddit { get; set; }

        public string Name { get; set; }
        public string Id { get; set; }
        public bool HasGold { get; set; }
        public bool IsModerator { get; set; }
        public int LinkKarma { get; set; }
        public int CommentKarma { get; set; }
        public DateTime Created { get; set; }

        public override string ToString()
        {
            return this.Name;
        }
    }
}
