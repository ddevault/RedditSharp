using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Security.Authentication;
using System.Text;
using Newtonsoft.Json.Linq;

namespace RedditSharp
{

    public class AuthProvider
    {
        private const string AccessUrl = "https://ssl.reddit.com/api/v1/access_token";
        private const string OauthGetMeUrl = "https://oauth.reddit.com/api/v1/me";
        private Reddit Reddit { get; set; }
        [Flags]
        public enum Scope
        {
            none=0x0,
            identity=0x1,
            edit = 0x2,
            flair = 0x4,
            history = 0x8,
            modconfig = 0x10,
            modflair = 0x20,
            modlog = 0x40,
            modposts = 0x80,
            modwiki = 0x100,
            mysubreddits = 0x200,
            privatemessages = 0x400,
            read = 0x800,
            report = 0x1000,
            save = 0x2000,
            submit = 0x4000,
            subscribe = 0x8000,
            vote = 0x10000,
            wikiedit = 0x20000,
            wikiread = 0x40000
        }
        private readonly IWebAgent _webAgent;
        private readonly string _redirectUri;
        private readonly string _clientID;

        public AuthProvider(Reddit reddit, string clientID, string redirectUri)
        {
            Reddit = reddit;
            _clientID = clientID;
            _redirectUri = redirectUri;
            _webAgent = new WebAgent();
        }

        public string GetAuthUrl(string state,Scope scope, bool permanent = false)
        {
            return String.Format("https://ssl.reddit.com/api/v1/authorize?client_id={0}&response_type=code&state={1}&redirect_uri={2}&duration={3}&scope={4}", _clientID, state, _redirectUri, permanent ? "permanent" : "temporary", scope);
        }

        public string GetOAuthToken(string code, string clientId, string clientSecret, bool isRefresh = false)
        {
            if (Type.GetType("Mono.Runtime") != null)
                ServicePointManager.ServerCertificateValidationCallback = (s, c, ch, ssl) => true;
            _webAgent.Cookies = new CookieContainer();

            var request = _webAgent.CreatePost(AccessUrl, false);

            request.Headers["Authorization"] = "Basic " + Convert.ToBase64String(Encoding.Default.GetBytes(clientId + ":" + clientSecret));
            var stream = request.GetRequestStream();

            _webAgent.WritePostBody(stream, new
            {
                grant_type = isRefresh ? "refresh_token" : "authorization_code",
                code,
                redirect_uri = _redirectUri
            });

            stream.Close();
            var response = (HttpWebResponse)request.GetResponse();
            var result = _webAgent.GetResponseString(response.GetResponseStream());
            var json = JObject.Parse(result);
            if (json["access_token"] != null)
                return json["access_token"].ToString();
            if (json["error"] != null)
            {
                throw new AuthenticationException("Could not log in: " + json["error"]);
            }
            throw new AuthenticationException("Could not log in.");
        }

        public AuthenticatedUser GetMe(string accessToken)
        {
            var request = _webAgent.CreateGet(OauthGetMeUrl, false);
            request.Headers["Authorization"] = String.Format("bearer {0}", accessToken);
            var response = (HttpWebResponse)request.GetResponse();
            var result = _webAgent.GetResponseString(response.GetResponseStream());
            var thingjson = "{\"kind\": \"t2\", \"data\": " + result + "}";
            var json = JObject.Parse(thingjson);

            return new AuthenticatedUser(Reddit, json, _webAgent);
        }
    }
}
