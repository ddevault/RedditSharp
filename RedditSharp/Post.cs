using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace RedditSharp
{
    public class Post
    {
        public string Author { get; set; }
        public string AuthorFlairClass { get; set; }
        public string AuthorFlairText { get; set; }
        public DateTime Created { get; set; }
        public string Domain { get; set; }
        public int Downvotes { get; set; }
        public bool Edited { get; set; }
        public bool Hidden { get; set; }
        public int Id { get; set; }
        public bool IsSelfPost { get; set; }
        public string LinkFlairClass { get; set; }
        public string LinkFlairText { get; set; }
        public int CommentCount { get; set; }
        public int ReportCount { get; set; }
        public bool NSFW { get; set; }
        public string Permalink { get; set; }
        public bool Saved { get; set; }
        public int Score { get; set; }
        public string SelfText { get; set; }
        public string SelfTextHtml { get; set; }
        public string Subreddit { get; set; }
        public string Thumbnail { get; set; }
        public string Title { get; set; }
        public int Upvotes { get; set; }
        public string Url { get; set; }
    }
}
