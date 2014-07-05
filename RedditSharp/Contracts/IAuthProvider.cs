using System;
using RedditSharp.Things;

namespace RedditSharp.Contracts
{
    public interface IAuthProvider
    {
        /// <summary>
        /// Creates the reddit OAuth2 Url to redirect the user to for authorization. 
        /// </summary>
        /// <param name="state">Used to verify that the user received is the user that was sent</param>
        /// <param name="scope">Determines what actions can be performed against the user.</param>
        /// <param name="permanent">Set to true for access lasting longer than one hour.</param>
        /// <returns></returns>
        string GetAuthUrl(string state, AuthProvider.Scope scope, bool permanent = false);

        /// <summary>
        /// Gets the OAuth token for the user associated with the provided code.
        /// </summary>
        /// <param name="code">Sent by reddit as a parameter in the return uri.</param>
        /// <param name="isRefresh">Set to true for refresh requests.</param>
        /// <returns></returns>
        string GetOAuthToken(string code, bool isRefresh = false);

        /// <summary>
        /// Gets a user authenticated by OAuth2.
        /// </summary>
        /// <param name="accessToken">Obtained using GetOAuthToken</param>
        /// <returns></returns>
        [Obsolete("Reddit.InitOrUpdateUser is preferred")]
        AuthenticatedUser GetUser(string accessToken);
    }
}