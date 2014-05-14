using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace RedditSharp
{
    public static class UriService
    {
        #region AuthenticatedUser
        public static Uri ModeratorUrl = new Uri("http://www.reddit.com/reddits/mine/moderator.json");
        public static Uri UnreadMessagesUrl = new Uri("http://www.reddit.com/message/unread.json?mark=true&limit=25");
        public static Uri ModQueueUrl = new Uri("http://www.reddit.com/r/mod/about/modqueue.json");
        public static Uri UnmoderatedUrl = new Uri("http://www.reddit.com/r/mod/about/unmoderated.json");
        public static Uri ModMailUrl = new Uri("http://www.reddit.com/message/moderator.json");
        public static Uri MessagesUrl = new Uri("http://www.reddit.com/message/messages.json");
        public static Uri InboxUrl = new Uri("http://www.reddit.com/message/inbox.json");
        public static Uri SentUrl = new Uri("http://www.reddit.com/message/sent.json");
        #endregion

        #region AuthProvider
        public static Uri AccessUrl = new Uri("https://ssl.reddit.com/api/v1/access_token");
        public static Uri OauthGetMeUrl = new Uri("https://oauth.reddit.com/api/v1/me");

        public static Uri AuthorizeUri = new Uri("https://ssl.reddit.com/api/v1/authorize");

        #endregion

        #region Captcha
        //public static Uri UrlFormat(string arg) { return new Uri(string.Format("http://www.reddit.com/captcha/{0}", arg)); }
        #endregion

        #region Comment
        public static Uri CommentUrl = new Uri("http://www.reddit.com/api/comment");
        public static Uri DistinguishUrl = new Uri("http://www.reddit.com/api/distinguish");
        public static Uri EditUserTextUrl = new Uri("http://www.reddit.com/api/editusertext");
        public static Uri RemoveUrl = new Uri("http://www.reddit.com/api/remove");
        public static Uri SetAsReadUrl = new Uri("http://www.reddit.com/api/read_message");
        #endregion

        #region Post
        // public static Uri CommentUrl = new Uri( "/api/comment");
        // public static Uri RemoveUrl = new Uri( "/api/remove");
        public static Uri DelUrl = new Uri("http://www.reddit.com/api/del");
        //public static Uri GetCommentsUrl = new Uri("http://www.reddit.com/comments/{0}.json");
        public static Uri ApproveUrl = new Uri("http://www.reddit.com/api/approve");
        // public static Uri EditUserTextUrl = new Uri( "/api/editusertext");
        public static Uri HideUrl = new Uri("http://www.reddit.com/api/hide");
        public static Uri UnhideUrl = new Uri("http://www.reddit.com/api/unhide");
        public static Uri SetFlairUrl = new Uri("http://www.reddit.com/api/flair");
        #endregion

        #region Private Message
        // public static Uri SetAsReadUrl = new Uri( "/api/read_message");
        // public static Uri CommentUrl = new Uri("http://www.reddit.com/api/comment");
        #endregion

        #region Reddit
        public static Uri SslLoginUrl = new Uri("https://ssl.reddit.com/api/login");
        public static Uri LoginUrl = new Uri("http://www.reddit.com/api/login/username");
        //public static Uri UserInfoUrl = new Uri("http://www.reddit.com/user/{0}/about.json");
        public static Uri MeUrl = new Uri("http://www.reddit.com/api/me.json");
        public static Uri OAuthMeUrl = new Uri("http://www.reddit.com/api/v1/me.json");
        //public static Uri SubredditAboutUrl = new Uri("http://www.reddit.com/r/{0}/about.json");
        public static Uri ComposeMessageUrl = new Uri("http://www.reddit.com/api/compose");
        public static Uri RegisterAccountUrl = new Uri("http://www.reddit.com/api/register");
        //public static Uri GetThingUrl = new Uri("http://www.reddit.com/by_id/{0}.json");
        //public static Uri GetCommentUrl = new Uri("http://www.reddit.com/r/{0}/comments/{1}/foo/{2}.json");
        //public static Uri GetPostUrl = new Uri("http://www.reddit.com/{0}.json");
        public static Uri DomainUrl = new Uri("http://www.reddit.com");
        public static Uri OAuthDomainUrl = new Uri("https://oauth.reddit.com");
        #endregion

        #region RedditUser
        //public static Uri OverviewUrl = new Uri("http://www.reddit.com/user/{0}.json");
        //public static Uri CommentsUrl = new Uri("http://www.reddit.com/user/{0}/comments.json");
        //public static Uri LinksUrl = new Uri("http://www.reddit.com/user/{0}/submitted.json");
        public static Uri SubscribedSubredditsUrl = new Uri("http://www.reddit.com/subreddits/mine.json");
        #endregion

        #region Subreddit
        //public static Uri SubredditPostUrl = new Uri("http://www.reddit.com/r/{0}.json");
        //public static Uri SubredditNewUrl = new Uri("http://www.reddit.com/r/{0}/new.json?sort=new");
        //public static Uri SubredditHotUrl = new Uri("http://www.reddit.com/r/{0}/hot.json");
        public static Uri SubscribeUrl = new Uri("http://www.reddit.com/api/subscribe");
        //public static Uri GetSettingsUrl = new Uri("http://www.reddit.com/r/{0}/about/edit.json");
        //public static Uri GetReducedSettingsUrl = new Uri("http://www.reddit.com/r/{0}/about.json");
        //public static Uri ModqueueUrl = new Uri("http://www.reddit.com/r/{0}/about/modqueue.json");
        // public static Uri UnmoderatedUrl = new Uri( "/r/{0}/about/unmoderated.json");
        public static Uri FlairTemplateUrl = new Uri("http://www.reddit.com/api/flairtemplate");
        public static Uri ClearFlairTemplatesUrl = new Uri("http://www.reddit.com/api/clearflairtemplates");
        public static Uri SetUserFlairUrl = new Uri("http://www.reddit.com/api/flair");
        //public static Uri StylesheetUrl = new Uri("http://www.reddit.com/r/{0}/about/stylesheet.json");
        public static Uri UploadImageUrl = new Uri("http://www.reddit.com/api/upload_sr_img");
        public static Uri FlairSelectorUrl = new Uri("http://www.reddit.com/api/flairselector");
        public static Uri AcceptModeratorInviteUrl = new Uri("http://www.reddit.com/api/accept_moderator_invite");
        public static Uri LeaveModerationUrl = new Uri("http://www.reddit.com/api/unfriend");
        public static Uri BanUserUrl = new Uri("http://www.reddit.com/api/friend");
        public static Uri AddModeratorUrl = new Uri("http://www.reddit.com/api/friend");
        public static Uri AddContributorUrl = new Uri("http://www.reddit.com/api/friend");
        //public static Uri ModeratorsUrl = new Uri("http://www.reddit.com/r/{0}/about/moderators.json");
        public static Uri FrontPageUrl = new Uri("http://www.reddit.com/.json");
        public static Uri SubmitLinkUrl = new Uri("http://www.reddit.com/api/submit");
        //public static Uri FlairListUrl = new Uri("http://www.reddit.com/r/{0}/api/flairlist.json");
        // public static Uri CommentsUrl = new Uri( "/r/{0}/comments.json");
        #endregion

        #region SubredditImage
        public static Uri DeleteImageUrl = new Uri("http://www.reddit.com/api/delete_sr_img");
        #endregion

        #region SubredditSettings
        public static Uri SiteAdminUrl = new Uri("http://www.reddit.com/api/site_admin");
        public static Uri DeleteHeaderImageUrl = new Uri("http://www.reddit.com/api/delete_sr_header");
        #endregion

        #region SubredditStyle
        // public static Uri UploadImageUrl = new Uri( "/api/upload_sr_img");
        public static Uri UpdateCssUrl = new Uri("http://www.reddit.com/api/subreddit_stylesheet");
        #endregion

        #region VotableThing
        public static Uri VoteUrl = new Uri("http://www.reddit.com/api/vote");
        public static Uri SaveUrl = new Uri("http://www.reddit.com/api/save");
        public static Uri UnsaveUrl = new Uri("http://www.reddit.com/api/unsave");
        #endregion

        #region Wiki
        //public static Uri GetWikiPageUrl = new Uri("http://www.reddit.com/r/{0}/wiki/{1}.json?v={2}");
        //public static Uri GetWikiPagesUrl = new Uri("http://www.reddit.com/r/{0}/wiki/pages.json");
        //public static Uri WikiPageEditUrl = new Uri("http://www.reddit.com/r/{0}/api/wiki/edit");
        //public static Uri HideWikiPageUrl = new Uri("http://www.reddit.com/r/{0}/api/wiki/hide");
        //public static Uri RevertWikiPageUrl = new Uri("http://www.reddit.com/r/{0}/api/wiki/revert");
        //public static Uri WikiPageAllowEditorAddUrl = new Uri("http://www.reddit.com/r/{0}/api/wiki/alloweditor/add");
        //public static Uri WikiPageAllowEditorDelUrl = new Uri("http://www.reddit.com/r/{0}/api/wiki/alloweditor/del");
        //public static Uri WikiPageSettingsUrl = new Uri("http://www.reddit.com/r/{0}/wiki/settings/{1}.json");
        //public static Uri WikiRevisionsUrl = new Uri("http://www.reddit.com/r/{0}/wiki/revisions.json");
        //public static Uri WikiPageRevisionsUrl = new Uri("http://www.reddit.com/r/{0}/wiki/revisions/{1}.json");
        //public static Uri WikiPageDiscussionsUrl = new Uri("http://www.reddit.com/r/{0}/wiki/discussions/{1}.json");
        #endregion
    }
}
