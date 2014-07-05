using RedditSharp.Helpers;

namespace RedditSharp.Models
{
    internal class TextData : SubmitData
    {
        [RedditAPIName("text")]
        internal string Text { get; set; }

        internal TextData()
        {
            Kind = "self";
        }
    }
}
