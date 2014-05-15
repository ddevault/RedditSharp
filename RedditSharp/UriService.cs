using System;
using System.ComponentModel;

namespace RedditSharp
{
    internal static class UriService
    {
        public static Uri GetUri(Endpoints endpoint, params object[] args)
        {
            Uri retval;
            var type = typeof(Endpoints);
            var memInfo = type.GetMember(endpoint.ToString());
            var attributes = memInfo[0].GetCustomAttributes(typeof(EndPointUrlAttribute),
                false);
            var endpointString = ((EndPointUrlAttribute)attributes[0]).EndpointString;
            return Uri.TryCreate(String.Format(endpointString, args), UriKind.RelativeOrAbsolute, out retval) ? retval : null;
        }

        internal enum Endpoints
        {
            #region AuthenticatedUser
            [EndPointUrl("http://www.reddit.com/reddits/mine/moderator.json")]
            ModeratorUrl,
            [EndPointUrl("http://www.reddit.com/message/unread.json?mark=true&limit=25")]
            UnreadMessagesUrl,
            [EndPointUrl("http://www.reddit.com/r/mod/about/modqueue.json")]
            ModQueueUrl,
            [EndPointUrl("http://www.reddit.com/r/mod/about/unmoderated.json")]
            AllUnmoderatedUrl,
            [EndPointUrl("http://www.reddit.com/message/moderator.json")]
            ModMailUrl,
            [EndPointUrl("http://www.reddit.com/message/messages.json")]
            MessagesUrl,
            [EndPointUrl("http://www.reddit.com/message/messages.json")]
            InboxUrl,
            [EndPointUrl("http://www.reddit.com/message/sent.json")]
            SentUrl,
            #endregion

            #region AuthProvider
            [EndPointUrl("https://ssl.reddit.com/api/v1/access_token")]
            AccessUrl,
            [EndPointUrl("https://oauth.reddit.com/api/v1/me")]
            OauthGetMeUrl,
            [EndPointUrl("https://ssl.reddit.com/api/v1/authorize")]
            AuthorizeUri,

            #endregion

            #region Captcha
            [EndPointUrl("http://www.reddit.com/captcha/{0}")]
            UrlFormat,
            #endregion

            #region Comment
            [EndPointUrl("http://www.reddit.com/api/comment")]
            CommentUrl,
            [EndPointUrl("http://www.reddit.com/api/distinguish")]
            DistinguishUrl,
            [EndPointUrl("http://www.reddit.com/api/editusertext")]
            EditUserTextUrl,
            [EndPointUrl("http://www.reddit.com/api/remove")]
            RemoveUrl,
            [EndPointUrl("http://www.reddit.com/api/read_message")]
            SetAsReadUrl,
            #endregion

            #region Post
            [EndPointUrl("http://www.reddit.com/api/del")]
            DelUrl,
            [EndPointUrl("http://www.reddit.com/comments/{0}.json")]
            GetCommentsUrl,
            [EndPointUrl("http://www.reddit.com/api/approve")]
            ApproveUrl,
            [EndPointUrl("http://www.reddit.com/api/hide")]
            HideUrl,
            [EndPointUrl("http://www.reddit.com/api/unhide")]
            UnhideUrl,
            [EndPointUrl("http://www.reddit.com/api/flair")]
            SetFlairUrl,
            #endregion

            #region Reddit
            [EndPointUrl("https://ssl.reddit.com/api/login")]
            SslLoginUrl,
            [EndPointUrl("http://www.reddit.com/api/login/username")]
            LoginUrl,
            [EndPointUrl("http://www.reddit.com/user/{0}/about.json")]
            UserInfoUrl,
            [EndPointUrl("http://www.reddit.com/api/me.json")]
            MeUrl,
            [EndPointUrl("http://www.reddit.com/api/v1/me.json")]
            OAuthMeUrl,
            [EndPointUrl("http://www.reddit.com/r/{0}/about.json")]
            SubredditAboutUrl,
            [EndPointUrl("http://www.reddit.com/api/compose")]
            ComposeMessageUrl,
            [EndPointUrl("http://www.reddit.com/api/register")]
            RegisterAccountUrl,
            [EndPointUrl("http://www.reddit.com/by_id/{0}.json")]
            GetThingUrl,
            [EndPointUrl("http://www.reddit.com/r/{0}/comments/{1}/foo/{2}.json")]
            GetCommentUrl,
            [EndPointUrl("http://www.reddit.com/{0}.json")]
            GetPostUrl,
            [EndPointUrl("http://www.reddit.com")]
            DomainUrl,
            [EndPointUrl("https://oauth.reddit.com")]
            OAuthDomainUrl,
            #endregion

            #region RedditUser
            [EndPointUrl("http://www.reddit.com/user/{0}.json")]
            OverviewUrl,
            [EndPointUrl("http://www.reddit.com/user/{0}/comments.json")]
            CommentsUrl,
            [EndPointUrl("http://www.reddit.com/user/{0}/submitted.json")]
            LinksUrl,
            [EndPointUrl("http://www.reddit.com/subreddits/mine.json")]
            SubscribedSubredditsUrl,
            #endregion

            #region Subreddit
            [EndPointUrl("http://www.reddit.com/r/{0}.json")]
            SubredditPostUrl,
            [EndPointUrl("http://www.reddit.com/r/{0}/new.json?sort=new")]
            SubredditNewUrl,
            [EndPointUrl("http://www.reddit.com/r/{0}/hot.json")]
            SubredditHotUrl,
            [EndPointUrl("http://www.reddit.com/api/subscribe")]
            SubscribeUrl,
            [EndPointUrl("http://www.reddit.com/r/{0}/about/edit.json")]
            GetSettingsUrl,
            [EndPointUrl("http://www.reddit.com/r/{0}/about.json")]
            GetReducedSettingsUrl,
            [EndPointUrl("http://www.reddit.com/r/{0}/about/modqueue.json")]
            ModqueueUrl,
            [EndPointUrl("http://www.reddit.com/r/{0}/about/unmoderated.json")]
            UnmoderatedUrl,
            [EndPointUrl("http://www.reddit.com/api/flairtemplate")]
            FlairTemplateUrl,
            [EndPointUrl("http://www.reddit.com/api/clearflairtemplates")]
            ClearFlairTemplatesUrl,
            [EndPointUrl("http://www.reddit.com/api/flair")]
            SetUserFlairUrl,
            [EndPointUrl("http://www.reddit.com/r/{0}/about/stylesheet.json")]
            StylesheetUrl,
            [EndPointUrl("http://www.reddit.com/api/upload_sr_img")]
            UploadImageUrl,
            [EndPointUrl("http://www.reddit.com/api/flairselector")]
            FlairSelectorUrl,
            [EndPointUrl("http://www.reddit.com/api/accept_moderator_invite")]
            AcceptModeratorInviteUrl,
            [EndPointUrl("http://www.reddit.com/api/unfriend")]
            LeaveModerationUrl,
            [EndPointUrl("http://www.reddit.com/api/friend")]
            BanUserUrl,
            [EndPointUrl("http://www.reddit.com/api/friend")]
            AddModeratorUrl,
            [EndPointUrl("http://www.reddit.com/api/friend")]
            AddContributorUrl,
            [EndPointUrl("http://www.reddit.com/r/{0}/about/moderators.json")]
            ModeratorsUrl,
            [EndPointUrl("http://www.reddit.com/.json")]
            FrontPageUrl,
            [EndPointUrl("http://www.reddit.com/api/submit")]
            SubmitLinkUrl,
            [EndPointUrl("http://www.reddit.com/r/{0}/api/flairlist.json")]
            FlairListUrl,
            #endregion

            #region SubredditImage
            [EndPointUrl("http://www.reddit.com/api/delete_sr_img")]
            DeleteImageUrl,
            #endregion

            #region SubredditSettings
            [EndPointUrl("http://www.reddit.com/api/site_admin")]
            SiteAdminUrl,
            [EndPointUrl("http://www.reddit.com/api/delete_sr_header")]
            DeleteHeaderImageUrl,
            #endregion

            #region SubredditStyle
            [EndPointUrl("http://www.reddit.com/api/subreddit_stylesheet")]
            UpdateCssUrl,
            #endregion

            #region VotableThing
            [EndPointUrl("http://www.reddit.com/api/vote")]
            VoteUrl,
            [EndPointUrl("http://www.reddit.com/api/save")]
            SaveUrl,
            [EndPointUrl("http://www.reddit.com/api/unsave")]
            UnsaveUrl,
            #endregion

            #region Wiki
            [EndPointUrl("http://www.reddit.com/r/{0}/wiki/{1}.json?v={2}")]
            GetWikiPageUrl,
            [EndPointUrl("http://www.reddit.com/r/{0}/wiki/pages.json")]
            GetWikiPagesUrl,
            [EndPointUrl("http://www.reddit.com/r/{0}/api/wiki/edit")]
            WikiPageEditUrl,
            [EndPointUrl("http://www.reddit.com/r/{0}/api/wiki/hide")]
            HideWikiPageUrl,
            [EndPointUrl("http://www.reddit.com/r/{0}/api/wiki/revert")]
            RevertWikiPageUrl,
            [EndPointUrl("http://www.reddit.com/r/{0}/api/wiki/alloweditor/add")]
            WikiPageAllowEditorAddUrl,
            [EndPointUrl("http://www.reddit.com/r/{0}/api/wiki/alloweditor/del")]
            WikiPageAllowEditorDelUrl,
            [EndPointUrl("http://www.reddit.com/r/{0}/wiki/settings/{1}.json")]
            WikiPageSettingsUrl,
            [EndPointUrl("http://www.reddit.com/r/{0}/wiki/revisions.json")]
            WikiRevisionsUrl,
            [EndPointUrl("http://www.reddit.com/r/{0}/wiki/revisions/{1}.json")]
            WikiPageRevisionsUrl,
            [EndPointUrl("http://www.reddit.com/r/{0}/wiki/discussions/{1}.json")]
            WikiPageDiscussionsUrl,
            #endregion
        }
    }

    public class EndPointUrlAttribute : Attribute
    {
        public EndPointUrlAttribute(string inputString)
        {
            EndpointString = inputString;
        }
        public string EndpointString { get; set; }
    }
}
