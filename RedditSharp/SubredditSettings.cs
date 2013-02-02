using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Web;
using Newtonsoft.Json.Linq;

namespace RedditSharp
{
    public class SubredditSettings
    {
        private const string SiteAdminUrl = "http://www.reddit.com/api/site_admin";
        private const string DeleteHeaderImageUrl = "http://www.reddit.com/api/delete_sr_header";

        private Reddit Reddit { get; set; }

        public Subreddit Subreddit { get; set; }

        public SubredditSettings(Subreddit subreddit, Reddit reddit, JObject json)
        {
            Subreddit = subreddit;
            Reddit = reddit;
            var data = json["data"];
            AllowAsDefault = data["default_set"].Value<bool>();
            Domain = data["domain"].Value<string>();
            Sidebar = HttpUtility.HtmlDecode(data["description"].Value<string>());
            Language = data["language"].Value<string>();
            Title = data["title"].Value<string>();
            WikiEditKarma = data["wiki_edit_karma"].Value<int>();
            UseDomainCss = data["domain_css"].Value<bool>();
            UseDomainSidebar = data["domain_sidebar"].Value<bool>();
            HeaderHoverText = data["header_hover_text"].Value<string>();
            NSFW = data["over_18"].Value<bool>();
            PublicDescription = HttpUtility.HtmlDecode(data["public_description"].Value<string>());
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
        public string Sidebar { get; set; }
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

        public void UpdateSettings()
        {
            var request = Reddit.CreatePost(SiteAdminUrl);
            var stream = request.GetRequestStream();
            string link_type;
            string type;
            string wikimode;
            switch (ContentOptions)
            {
                case RedditSharp.ContentOptions.All:
                    link_type = "any";
                    break;
                case RedditSharp.ContentOptions.LinkOnly:
                    link_type = "link";
                    break;
                default:
                    link_type = "self";
                    break;
            }
            switch (SubredditType)
            {
                case SubredditType.Public:
                    type = "public";
                    break;
                case SubredditType.Private:
                    type = "private";
                    break;
                default:
                    type = "restricted";
                    break;
            }
            switch (WikiEditMode)
            {
                case WikiEditMode.All:
                    wikimode = "anyone";
                    break;
                case WikiEditMode.Moderators:
                    wikimode = "modonly";
                    break;
                default:
                    wikimode = "disabled";
                    break;
            }
            Reddit.WritePostBody(stream, new
            {
                allow_top = AllowAsDefault,
                description = Sidebar,
                domain = Domain,
                lang = Language,
                link_type,
                over_18 = NSFW,
                public_description = PublicDescription,
                show_media = ShowThumbnails,
                sr = Subreddit.FullName,
                title = Title,
                type,
                uh = Reddit.User.Modhash,
                wiki_edit_age = WikiEditAge,
                wiki_edit_karma = WikiEditKarma,
                wikimode,
                api_type = "json"
            }, "header-title", HeaderHoverText);
            stream.Close();
            var response = request.GetResponse();
            var data = Reddit.GetResponseString(response.GetResponseStream());
        }

        /// <summary>
        /// Resets the subreddit's header image to the Reddit logo
        /// </summary>
        public void ResetHeaderImage()
        {
            var request = Reddit.CreatePost(DeleteHeaderImageUrl);
            var stream = request.GetRequestStream();
            Reddit.WritePostBody(stream, new
            {
                uh = Reddit.User.Modhash,
                r = Subreddit.Name
            });
            stream.Close();
            var response = request.GetResponse();
            var data = Reddit.GetResponseString(response.GetResponseStream());
        }
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
