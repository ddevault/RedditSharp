using System.IO;
using System.Net;
using System.Threading.Tasks;

namespace RedditSharp
{
    public interface IWebAgent
    {
        bool IsAsync { get; }
        CookieContainer Cookies { get; set; }
        string AuthCookie { get; set; }
        HttpWebRequest CreateRequest(string url, string method);
        HttpWebRequest CreateGet(string url);
        HttpWebRequest CreatePost(string url);
        string GetResponseString(Stream stream);
        void WritePostBody(Stream stream, object data, params string[] additionalFields);   
    }

    public interface IAsyncWebAgent : IWebAgent 
    {
        Task<HttpWebRequest> CreateRequestAsync(string url, string method);
        Task<HttpWebRequest> CreateGetAsync(string url);
        Task<HttpWebRequest> CreatePostAsync(string url);
        Task<string> GetResponseStringAsync(Stream stream);
        Task WritePostBodyAsync(Stream stream, object data, params string[] additionalFields);
    }
}