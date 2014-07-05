using RedditSharp.Helpers;

namespace RedditSharp.Models
{
    internal class LinkData : SubmitData
    {
        [RedditAPIName("extension")]
        internal string Extension { get; set; }
        
        [RedditAPIName("url")]
        internal string URL { get; set; }

        internal LinkData()
        {
            Extension = "json";
            Kind = "link";
        }
    }
}
