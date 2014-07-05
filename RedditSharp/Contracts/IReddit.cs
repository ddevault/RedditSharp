using System;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using RedditSharp.Things;

namespace RedditSharp.Contracts
{
    public interface IReddit
    {
        /// <summary>
        /// Captcha solver instance to use when solving captchas.
        /// </summary>
        ICaptchaSolver CaptchaSolver { get; set; }
        /// <summary>
        /// The authenticated user for this instance.
        /// </summary>
        AuthenticatedUser User { get; set; }

        /// <summary>
        /// Sets the Rate Limiting Mode of the underlying WebAgent
        /// </summary>
        WebAgent.RateLimitMode RateLimit { get; set; }

        /// <summary>
        /// Gets the FrontPage using the current Reddit instance.
        /// </summary>
        Subreddit FrontPage { get; }

        /// <summary>
        /// Gets /r/All using the current Reddit instance.
        /// </summary>
        Subreddit RSlashAll { get; }

        /// <summary>
        /// Logs in the current Reddit instance.
        /// </summary>
        /// <param name="username">The username of the user to log on to.</param>
        /// <param name="password">The password of the user to log on to.</param>
        /// <param name="useSsl">Whether to use SSL or not. (default: true)</param>
        /// <returns></returns>
        AuthenticatedUser LogIn(string username, string password, bool useSsl = true);

        RedditUser GetUser(string name);

        /// <summary>
        /// Initializes the User property if it's null,
        /// otherwise replaces the existing user object
        /// with a new one fetched from reddit servers.
        /// </summary>
        void InitOrUpdateUser();

        [Obsolete("Use User property instead")]
        AuthenticatedUser GetMe();

        Subreddit GetSubreddit(string name);
        Domain GetDomain(string domain);
        JToken GetToken(Uri uri);
        Post GetPost(Uri uri);
        void ComposePrivateMessage(string subject, string body, string to, string captchaId = "", string captchaAnswer = "");

        /// <summary>
        /// Registers a new Reddit user
        /// </summary>
        /// <param name="userName">The username for the new account.</param>
        /// <param name="passwd">The password for the new account.</param>
        /// <param name="email">The optional recovery email for the new account.</param>
        /// <returns>The newly created user account</returns>
        AuthenticatedUser RegisterAccount(string userName, string passwd, string email = "");

        Thing GetThingByFullname(string fullname);
        Comment GetComment(string subreddit, string name, string linkName);

        JsonSerializerSettings JsonSerializerSettings { get; set; }
    }
}