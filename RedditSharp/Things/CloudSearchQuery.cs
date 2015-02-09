using System;

namespace RedditSharp.Things
{
    public class CloudSearchQuery
    {
        public string Title { get; set; }
        public DateTime? DateTimeFrom { get; set; }
        public DateTime? DateTimeTo { get; set; }
    }
}