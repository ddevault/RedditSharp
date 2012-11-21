using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Newtonsoft.Json.Linq;

namespace RedditSharp
{
    public class Comment
    {
        private Reddit Reddit { get; set; }

        internal static Comment FromPost(Reddit reddit, JToken json)
        {
            var comment = new Comment();
            var data = json["data"];
            comment.Content = data["contentText"].Value<string>();
            comment.ContentHtml = data["contentHTML"].Value<string>();
            comment.Id = data["id"].Value<string>();
            return comment;
        }

        private Comment()
        {
        }

        public Comment(Reddit reddit, JToken json)
        {
            // TODO
        }

        public string Author { get; set; }
        public string Content { get; set; }
        public string ContentHtml { get; set; }
        public string Id { get; set; }
        public string ParentId { get; set; }
    }
}
