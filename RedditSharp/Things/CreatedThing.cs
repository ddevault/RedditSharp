using System;
using System.Threading.Tasks;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace RedditSharp.Things
{
    public class CreatedThing : Thing
    {
        private Reddit Reddit { get; set; }

        protected CreatedThing Init(Reddit reddit, JToken json)
        {
            CommonInit(reddit, json);
            JsonConvert.PopulateObject(json["data"].ToString(), this, reddit.JsonSerializerSettings);
            return this;
        }
        protected async Task<CreatedThing> InitAsync(Reddit reddit, JToken json)
        {
            CommonInit(reddit, json);
            await Task.Factory.StartNew(() => JsonConvert.PopulateObject(json["data"].ToString(), this, reddit.JsonSerializerSettings));
            return this;
        }

        private void CommonInit(Reddit reddit, JToken json)
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
