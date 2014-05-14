using System;
using System.IO;
using System.Net;
using Newtonsoft.Json.Linq;

namespace RedditSharp
{
    public interface IWebAgent
    {
        CookieContainer Cookies { get; set; }
        string AuthCookie { get; set; }
        string AccessToken { get; set; }
        [Obsolete("The Uri implementation of this method is preferred.")]
        HttpWebRequest CreateRequest(string url, string method);
        [Obsolete("The Uri implementation of this method is preferred.")]
        HttpWebRequest CreateGet(string url);
        [Obsolete("The Uri implementation of this method is preferred.")]
        HttpWebRequest CreatePost(string url);
        HttpWebRequest CreateRequest(Uri url, string method);
        HttpWebRequest CreateGet(Uri url);
        HttpWebRequest CreatePost(Uri url);
        string GetResponseString(Stream stream);
        void WritePostBody(Stream stream, object data, params string[] additionalFields);
        JToken CreateAndExecuteRequest(Uri url);
        JToken ExecuteRequest(HttpWebRequest request);
    }
}
