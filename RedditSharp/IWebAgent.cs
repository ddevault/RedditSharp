using System.IO;
using System.Net;

namespace RedditSharp
{
    public interface IWebAgent
    {
        CookieContainer Cookies { get; set; }
        string AuthCookie { get; set; }
        HttpWebRequest CreateRequest(string url, string method, bool prependDomain = true);
        HttpWebRequest CreateGet(string url, bool prependDomain = true);
        HttpWebRequest CreatePost(string url, bool prependDomain = true);
        string GetResponseString(Stream stream);
        void WritePostBody(Stream stream, object data, params string[] additionalFields);
    }
}