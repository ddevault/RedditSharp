using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Newtonsoft.Json.Linq;

namespace RedditSharp
{
    public class RedditUser
    {
        protected RedditUser() { }

        public RedditUser(JToken json)
        {
            var data = json["data"];
            Name = data["name"].Value<string>();
            Id = data["id"].Value<string>();
            HasGold = data["is_gold"].Value<bool>();
            IsModerator = data["is_mod"].Value<bool>();
            LinkKarma = data["link_karma"].Value<int>();
            CommentKarma = data["comment_karma"].Value<int>();
            Created = Reddit.UnixTimeStampToDateTime(data["created"].Value<double>());
        }

        public string Name { get; set; }
        public string Id { get; set; }
        public bool HasGold { get; set; }
        public bool IsModerator { get; set; }
        public int LinkKarma { get; set; }
        public int CommentKarma { get; set; }
        public DateTime Created { get; set; }
    }
}