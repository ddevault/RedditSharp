using System.IO;
using System.Net;
using System.Threading.Tasks;

namespace RedditSharp
{
    public interface IWebAgent
    {
        CookieContainer Cookies { get; set; }
        string AuthCookie { get; set; }
        HttpWebRequest CreateRequest(string url, string method);
        HttpWebRequest CreateGet(string url);
        HttpWebRequest CreatePost(string url);
        string GetResponseString(Stream stream);
        Task<string> GetResponseStringAsync(Stream stream);
        void WritePostBody(Stream stream, object data, params string[] additionalFields);
        Task WritePostBodyAsync(Stream stream, object data, params string[] additionalFields);
    }
}