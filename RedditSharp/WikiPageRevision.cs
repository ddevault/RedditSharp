using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;

namespace RedditSharp
{
    public class WikiPageRevision : Thing
    {
        [JsonProperty("id")]
        new public string Id { get; private set; }

        [JsonProperty("timestamp")]
        [JsonConverter(typeof(UnixTimestampConverter))]
        public DateTime? TimeStamp { get; set; }

        [JsonProperty("reason")]
        public string Reason { get; private set; }

        [JsonProperty("page")]
        public string Page { get; private set; }

        [JsonIgnore]
        public RedditUser Author { get; set; }

        protected internal WikiPageRevision(Reddit reddit, JToken json, IWebAgent webAgent)
            : base(null)
        {
            Author = new RedditUser(reddit, json["author"], webAgent);
            JsonConvert.PopulateObject(json.ToString(), this, reddit.JsonSerializerSettings);
        }
    }
}