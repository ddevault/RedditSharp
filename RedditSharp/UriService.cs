using System;

namespace RedditSharp
{
    public static class UriService
    {
        public static Uri GetUri(Endpoints endpoint, params object[] args)
        {
            switch (endpoint)
            {
                #region AuthenticatedUser
                case Endpoints.ModeratorUrl: return new Uri("http://www.reddit.com/reddits/mine/moderator.json");
                case Endpoints.UnreadMessagesUrl: return new Uri("http://www.reddit.com/message/unread.json?mark=true&limit=25");
                case Endpoints.ModQueueUrl: return new Uri("http://www.reddit.com/r/mod/about/modqueue.json");
                case Endpoints.AllUnmoderatedUrl: return new Uri("http://www.reddit.com/r/mod/about/unmoderated.json");
                case Endpoints.ModMailUrl: return new Uri("http://www.reddit.com/message/moderator.json");
                case Endpoints.MessagesUrl: return new Uri("http://www.reddit.com/message/messages.json");
                case Endpoints.InboxUrl: return new Uri("http://www.reddit.com/message/inbox.json");
                case Endpoints.SentUrl: return new Uri("http://www.reddit.com/message/sent.json");
                #endregion

                #region AuthProvider
                case Endpoints.AccessUrl: return new Uri("https://ssl.reddit.com/api/v1/access_token");
                case Endpoints.OauthGetMeUrl: return new Uri("https://oauth.reddit.com/api/v1/me");

                case Endpoints.AuthorizeUri: return new Uri("https://ssl.reddit.com/api/v1/authorize");

                #endregion

                #region Captcha
                case Endpoints.UrlFormat: return new Uri(string.Format("http://www.reddit.com/captcha/{0}", args[0]));
                #endregion

                #region Comment
                case Endpoints.CommentUrl: return new Uri("http://www.reddit.com/api/comment");
                case Endpoints.DistinguishUrl: return new Uri("http://www.reddit.com/api/distinguish");
                case Endpoints.EditUserTextUrl: return new Uri("http://www.reddit.com/api/editusertext");
                case Endpoints.RemoveUrl: return new Uri("http://www.reddit.com/api/remove");
                case Endpoints.SetAsReadUrl: return new Uri("http://www.reddit.com/api/read_message");
                #endregion

                #region Post
                case Endpoints.DelUrl: return new Uri("http://www.reddit.com/api/del");
                case Endpoints.GetCommentsUrl: return new Uri(string.Format("http://www.reddit.com/comments/{0}.json", args[0]));
                case Endpoints.ApproveUrl: return new Uri("http://www.reddit.com/api/approve");
                case Endpoints.HideUrl: return new Uri("http://www.reddit.com/api/hide");
                case Endpoints.UnhideUrl: return new Uri("http://www.reddit.com/api/unhide");
                case Endpoints.SetFlairUrl: return new Uri("http://www.reddit.com/api/flair");
                #endregion

                #region Reddit
                case Endpoints.SslLoginUrl: return new Uri("https://ssl.reddit.com/api/login");
                case Endpoints.LoginUrl: return new Uri("http://www.reddit.com/api/login/username");
                case Endpoints.UserInfoUrl: return new Uri(string.Format("http://www.reddit.com/user/{0}/about.json", args[0]));
                case Endpoints.MeUrl: return new Uri("http://www.reddit.com/api/me.json");
                case Endpoints.OAuthMeUrl: return new Uri("http://www.reddit.com/api/v1/me.json");
                case Endpoints.SubredditAboutUrl: return new Uri(string.Format("http://www.reddit.com/r/{0}/about.json", args[0]));
                case Endpoints.ComposeMessageUrl: return new Uri("http://www.reddit.com/api/compose");
                case Endpoints.RegisterAccountUrl: return new Uri("http://www.reddit.com/api/register");
                case Endpoints.GetThingUrl: return new Uri(string.Format("http://www.reddit.com/by_id/{0}.json", args[0]));
                case Endpoints.GetCommentUrl: return new Uri(string.Format("http://www.reddit.com/r/{0}/comments/{1}/foo/{2}.json", args[0], args[1], args[2]));
                case Endpoints.GetPostUrl: return new Uri(string.Format("http://www.reddit.com/{0}.json", args[0]));
                case Endpoints.DomainUrl: return new Uri("http://www.reddit.com");
                case Endpoints.OAuthDomainUrl: return new Uri("https://oauth.reddit.com");
                #endregion

                #region RedditUser
                case Endpoints.OverviewUrl: return new Uri(string.Format("http://www.reddit.com/user/{0}.json", args[0]));
                case Endpoints.CommentsUrl: return new Uri(string.Format("http://www.reddit.com/user/{0}/comments.json", args[0]));
                case Endpoints.LinksUrl: return new Uri(string.Format("http://www.reddit.com/user/{0}/submitted.json", args[0]));
                case Endpoints.SubscribedSubredditsUrl: return new Uri("http://www.reddit.com/subreddits/mine.json");
                #endregion

                #region Subreddit
                case Endpoints.SubredditPostUrl: return new Uri(string.Format("http://www.reddit.com/r/{0}.json", args[0]));
                case Endpoints.SubredditNewUrl: return new Uri(string.Format("http://www.reddit.com/r/{0}/new.json?sort=new", args[0]));
                case Endpoints.SubredditHotUrl: return new Uri(string.Format("http://www.reddit.com/r/{0}/hot.json", args[0]));
                case Endpoints.SubscribeUrl: return new Uri("http://www.reddit.com/api/subscribe");
                case Endpoints.GetSettingsUrl: return new Uri(string.Format("http://www.reddit.com/r/{0}/about/edit.json", args[0]));
                case Endpoints.GetReducedSettingsUrl: return new Uri(string.Format("http://www.reddit.com/r/{0}/about.json", args[0]));
                case Endpoints.ModqueueUrl: return new Uri(string.Format("http://www.reddit.com/r/{0}/about/modqueue.json", args[0]));
                case Endpoints.UnmoderatedUrl: return new Uri(string.Format("http://www.reddit.com/r/{0}/about/unmoderated.json", args[0]));
                case Endpoints.FlairTemplateUrl: return new Uri("http://www.reddit.com/api/flairtemplate");
                case Endpoints.ClearFlairTemplatesUrl: return new Uri("http://www.reddit.com/api/clearflairtemplates");
                case Endpoints.SetUserFlairUrl: return new Uri("http://www.reddit.com/api/flair");
                case Endpoints.StylesheetUrl: return new Uri(string.Format("http://www.reddit.com/r/{0}/about/stylesheet.json", args[0]));
                case Endpoints.UploadImageUrl: return new Uri("http://www.reddit.com/api/upload_sr_img");
                case Endpoints.FlairSelectorUrl: return new Uri("http://www.reddit.com/api/flairselector");
                case Endpoints.AcceptModeratorInviteUrl: return new Uri("http://www.reddit.com/api/accept_moderator_invite");
                case Endpoints.LeaveModerationUrl: return new Uri("http://www.reddit.com/api/unfriend");
                case Endpoints.BanUserUrl: return new Uri("http://www.reddit.com/api/friend");
                case Endpoints.AddModeratorUrl: return new Uri("http://www.reddit.com/api/friend");
                case Endpoints.AddContributorUrl: return new Uri("http://www.reddit.com/api/friend");
                case Endpoints.ModeratorsUrl: return new Uri(string.Format("http://www.reddit.com/r/{0}/about/moderators.json", args[0]));
                case Endpoints.FrontPageUrl: return new Uri("http://www.reddit.com/.json");
                case Endpoints.SubmitLinkUrl: return new Uri("http://www.reddit.com/api/submit");
                case Endpoints.FlairListUrl: return new Uri(string.Format("http://www.reddit.com/r/{0}/api/flairlist.json", args[0]));
                #endregion

                #region SubredditImage
                case Endpoints.DeleteImageUrl: return new Uri("http://www.reddit.com/api/delete_sr_img");
                #endregion

                #region SubredditSettings
                case Endpoints.SiteAdminUrl: return new Uri("http://www.reddit.com/api/site_admin");
                case Endpoints.DeleteHeaderImageUrl: return new Uri("http://www.reddit.com/api/delete_sr_header");
                #endregion

                #region SubredditStyle
                case Endpoints.UpdateCssUrl: return new Uri("http://www.reddit.com/api/subreddit_stylesheet");
                #endregion

                #region VotableThing
                case Endpoints.VoteUrl: return new Uri("http://www.reddit.com/api/vote");
                case Endpoints.SaveUrl: return new Uri("http://www.reddit.com/api/save");
                case Endpoints.UnsaveUrl: return new Uri("http://www.reddit.com/api/unsave");
                #endregion

                #region Wiki
                case Endpoints.GetWikiPageUrl: return new Uri(string.Format("http://www.reddit.com/r/{0}/wiki/{1}.json?v={2}", args[0], args[1], args[2]));
                case Endpoints.GetWikiPagesUrl: return new Uri(string.Format("http://www.reddit.com/r/{0}/wiki/pages.json", args[0]));
                case Endpoints.WikiPageEditUrl: return new Uri(string.Format("http://www.reddit.com/r/{0}/api/wiki/edit", args[0]));
                case Endpoints.HideWikiPageUrl: return new Uri(string.Format("http://www.reddit.com/r/{0}/api/wiki/hide", args[0]));
                case Endpoints.RevertWikiPageUrl: return new Uri(string.Format("http://www.reddit.com/r/{0}/api/wiki/revert", args[0]));
                case Endpoints.WikiPageAllowEditorAddUrl: return new Uri(string.Format("http://www.reddit.com/r/{0}/api/wiki/alloweditor/add", args[0]));
                case Endpoints.WikiPageAllowEditorDelUrl: return new Uri(string.Format("http://www.reddit.com/r/{0}/api/wiki/alloweditor/del", args[0]));
                case Endpoints.WikiPageSettingsUrl: return new Uri(string.Format("http://www.reddit.com/r/{0}/wiki/settings/{1}.json", args[0], args[1]));
                case Endpoints.WikiRevisionsUrl: return new Uri(string.Format("http://www.reddit.com/r/{0}/wiki/revisions.json", args[0]));
                case Endpoints.WikiPageRevisionsUrl: return new Uri(string.Format("http://www.reddit.com/r/{0}/wiki/revisions/{1}.json", args[0], args[1]));
                case Endpoints.WikiPageDiscussionsUrl: return new Uri(string.Format("http://www.reddit.com/r/{0}/wiki/discussions/{1}.json", args[0], args[1]));
                #endregion

            }
            return null;
        }



        public enum Endpoints
        {
            #region AuthenticatedUser
            ModeratorUrl,
            UnreadMessagesUrl,
            ModQueueUrl,
            AllUnmoderatedUrl,
            ModMailUrl,
            MessagesUrl,
            InboxUrl,
            SentUrl,
            #endregion

            #region AuthProvider
            AccessUrl,
            OauthGetMeUrl,

            AuthorizeUri,

            #endregion

            #region Captcha
            UrlFormat,
            #endregion

            #region Comment
            CommentUrl,
            DistinguishUrl,
            EditUserTextUrl,
            RemoveUrl,
            SetAsReadUrl,
            #endregion

            #region Post
            DelUrl,
            GetCommentsUrl,
            ApproveUrl,
            HideUrl,
            UnhideUrl,
            SetFlairUrl,
            #endregion

            #region Reddit
            SslLoginUrl,
            LoginUrl,
            UserInfoUrl,
            MeUrl,
            OAuthMeUrl,
            SubredditAboutUrl,
            ComposeMessageUrl,
            RegisterAccountUrl,
            GetThingUrl,
            GetCommentUrl,
            GetPostUrl,
            DomainUrl,
            OAuthDomainUrl,
            #endregion

            #region RedditUser
            OverviewUrl,
            CommentsUrl,
            LinksUrl,
            SubscribedSubredditsUrl,
            #endregion

            #region Subreddit
            SubredditPostUrl,
            SubredditNewUrl,
            SubredditHotUrl,
            SubscribeUrl,
            GetSettingsUrl,
            GetReducedSettingsUrl,
            ModqueueUrl,
            UnmoderatedUrl,
            FlairTemplateUrl,
            ClearFlairTemplatesUrl,
            SetUserFlairUrl,
            StylesheetUrl,
            UploadImageUrl,
            FlairSelectorUrl,
            AcceptModeratorInviteUrl,
            LeaveModerationUrl,
            BanUserUrl,
            AddModeratorUrl,
            AddContributorUrl,
            ModeratorsUrl,
            FrontPageUrl,
            SubmitLinkUrl,
            FlairListUrl,
            #endregion

            #region SubredditImage
            DeleteImageUrl,
            #endregion

            #region SubredditSettings
            SiteAdminUrl,
            DeleteHeaderImageUrl,
            #endregion

            #region SubredditStyle
            UpdateCssUrl,
            #endregion

            #region VotableThing
            VoteUrl,
            SaveUrl,
            UnsaveUrl,
            #endregion

            #region Wiki
            GetWikiPageUrl,
            GetWikiPagesUrl,
            WikiPageEditUrl,
            HideWikiPageUrl,
            RevertWikiPageUrl,
            WikiPageAllowEditorAddUrl,
            WikiPageAllowEditorDelUrl,
            WikiPageSettingsUrl,
            WikiRevisionsUrl,
            WikiPageRevisionsUrl,
            WikiPageDiscussionsUrl,
            #endregion
        }
    }
}
