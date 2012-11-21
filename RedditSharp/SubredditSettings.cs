using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Newtonsoft.Json.Linq;

namespace RedditSharp
{
    public class SubredditSettings
    {
        private Reddit Reddit { get; set; }

        public Subreddit Subreddit { get; set; }

        public SubredditSettings(Subreddit subreddit, Reddit reddit, JObject json)
        {
            Subreddit = subreddit;
            Reddit = reddit;
            var data = json["data"];
            AllowAsDefault = data["default_set"].Value<bool>();
            Domain = data["domain"].Value<string>();
            Description = data["description"].Value<string>();
            Language = data["language"].Value<string>();
            Title = data["title"].Value<string>();
            WikiEditKarma = data["wiki_edit_karma"].Value<int>();
            UseDomainCss = data["domain_css"].Value<bool>();
            UseDomainSidebar = data["domain_sidebar"].Value<bool>();
            HeaderHoverText = data["header_hover_text"].Value<string>();
            NSFW = data["over_18"].Value<bool>();
            PublicDescription = data["public_description"].Value<string>();
            string wikiMode = data["wikimode"].Value<string>();
            switch (wikiMode)
            {
                case "disabled":
                    WikiEditMode = WikiEditMode.None;
                    break;
                case "modonly":
                    WikiEditMode = WikiEditMode.Moderators;
                    break;
                case "anyone":
                    WikiEditMode = WikiEditMode.All;
                    break;
            }
            string type = data["subreddit_type"].Value<string>();
            switch (type)
            {
                case "public":
                    SubredditType = SubredditType.Public;
                    break;
                case "private":
                    SubredditType = SubredditType.Private;
                    break;
                case "restricted":
                    SubredditType = SubredditType.Restricted;
                    break;
            }
            ShowThumbnails = data["show_media"].Value<bool>();
            WikiEditAge = data["wiki_edit_age"].Value<int>();
            string contentOptions = data["content_options"].Value<string>();
            switch (contentOptions)
            {
                case "any":
                    ContentOptions = ContentOptions.All;
                    break;
                case "link":
                    ContentOptions = ContentOptions.LinkOnly;
                    break;
                case "self":
                    ContentOptions = ContentOptions.SelfOnly;
                    break;
            }
        }

        public bool AllowAsDefault { get; set; }
        public string Domain { get; set; }
        public string Description { get; set; }
        public string Language { get; set; }
        public string Title { get; set; }
        public int WikiEditKarma { get; set; }
        public bool UseDomainCss { get; set; }
        public bool UseDomainSidebar { get; set; }
        public string HeaderHoverText { get; set; }
        public bool NSFW { get; set; }
        public string PublicDescription { get; set; }
        public WikiEditMode WikiEditMode { get; set; }
        public SubredditType SubredditType { get; set; }
        public bool ShowThumbnails { get; set; }
        public int WikiEditAge { get; set; }
        public ContentOptions ContentOptions { get; set; }
    }

    public enum WikiEditMode
    {
        None,
        Moderators,
        All
    }

    public enum SubredditType
    {
        Public,
        Restricted,
        Private
    }

    public enum ContentOptions
    {
        All,
        LinkOnly,
        SelfOnly
    }
}
