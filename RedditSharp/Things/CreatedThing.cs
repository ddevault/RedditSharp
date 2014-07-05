using System;
using System.Threading.Tasks;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using RedditSharp.Contracts;

namespace RedditSharp.Things
{
    public class CreatedThing : Thing
    {
        private Contracts.IReddit Reddit { get; set; }

        protected CreatedThing Init(IReddit reddit, JToken json)
        {
            CommonInit(reddit, json);
            JsonConvert.PopulateObject(json["data"].ToString(), this, reddit.JsonSerializerSettings);
            return this;
        }
        protected async Task<CreatedThing> InitAsync(IReddit reddit, JToken json)
        {
            CommonInit(reddit, json);
            await JsonConvert.PopulateObjectAsync(json["data"].ToString(), this, reddit.JsonSerializerSettings);
            return this;
        }

        private void CommonInit(Contracts.IReddit reddit, JToken json)
        {
            base.Init(json);
            Reddit = reddit;
        }


        [JsonProperty("created")]
        [JsonConverter(typeof(UnixTimestampConverter))]
        public DateTime Created { get; set; }

        [JsonProperty("created_utc")]
        [JsonConverter(typeof(UnixTimestampConverter))]
        public DateTime CreatedUTC { get; set; }
    }
}
