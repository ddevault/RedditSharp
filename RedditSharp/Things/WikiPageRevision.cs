using System;
using System.Threading.Tasks;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using RedditSharp.Contracts;

namespace RedditSharp.Things
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

        protected internal WikiPageRevision() { }

        internal WikiPageRevision Init(IReddit reddit, JToken json, IWebAgent webAgent)
        {
            CommonInit(reddit, json, webAgent);
            JsonConvert.PopulateObject(json.ToString(), this, reddit.JsonSerializerSettings);
            return this;
        }        

        internal async Task<WikiPageRevision> InitAsync(IReddit reddit, JToken json, IWebAgent webAgent)
        {
            CommonInit(reddit, json, webAgent);
            await JsonConvert.PopulateObjectAsync(json.ToString(), this, reddit.JsonSerializerSettings);
            return this;
        }

        private void CommonInit(IReddit reddit, JToken json, IWebAgent webAgent)
        {
            base.Init(json);
            Author = new RedditUser().Init(reddit, json["author"], webAgent);
        }
    }
}