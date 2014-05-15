using System;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace RedditSharp
{
    public class Domain
    {
        private const string DomainPostUrl = "/domain/{0}.json";
        private const string DomainNewUrl = "/domain/{0}/new.json?sort=new";
        private const string DomainHotUrl = "/domain/{0}/hot.json";
        private const string FrontPageUrl = "/.json";

        [JsonIgnore]
        private Reddit Reddit { get; set; }

        [JsonIgnore]
        private IWebAgent WebAgent { get; set; }

        [JsonIgnore]
        public string Name { get; set; }

        public Listing<Post> Posts
        {
            get
            {
                return new Listing<Post>(Reddit, string.Format(DomainPostUrl, Name), WebAgent);
            }
        }

        public Listing<Post> New
        {
            get
            {
                return new Listing<Post>(Reddit, string.Format(DomainNewUrl, Name), WebAgent);
            }
        }

        public Listing<Post> Hot
        {
            get
            {
                return new Listing<Post>(Reddit, string.Format(DomainHotUrl, Name), WebAgent);
            }
        }

        protected internal Domain(Reddit reddit, Uri domain, IWebAgent webAgent)
        {
            Reddit = reddit;
            WebAgent = webAgent;
            Name = domain.Host;
        }

        public override string ToString()
        {
            return "/domain/" + Name;
        }
    }
}

