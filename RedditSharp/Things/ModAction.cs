using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RedditSharp.Things
{
    public class ModAction : Thing
    {

        [JsonProperty("action")]
        [JsonConverter(typeof(ModActionTypeConverter))]
        public ModActionType Action { get; set; }

        [JsonProperty("created_utc")]
        [JsonConverter(typeof(UnixTimestampConverter))]
        public DateTime? TimeStamp { get; set; }

        [JsonProperty("details")]
        public string Details { get; set; }

        [JsonProperty("mod")]
        public string ModeratorName { get; set; }

        [JsonProperty("target_author")]
        public string TargetAuthorName { get; set; }

        [JsonProperty("target_fullname")]
        public string TargetThingFullname { get; set; }

        [JsonProperty("target_permalink")]
        public string TargetThingPermalink { get; set; }

        [JsonIgnore]
        public RedditUser TargetAuthor
        {
            get
            {
                return Reddit.GetUser(TargetAuthorName);
            }
        }

        [JsonIgnore]
        public Thing TargetThing
        {
            get
            {
                return Reddit.GetThingByFullname(TargetThingFullname);
            }
        }

        [JsonIgnore]
        private Reddit Reddit { get; set; }

        [JsonIgnore]
        private IWebAgent WebAgent { get; set; }

        public ModAction Init(Reddit reddit, JToken json, IWebAgent webAgent)
        {
            CommonInit(reddit, json, webAgent);
            JsonConvert.PopulateObject(json["data"].ToString(), this, reddit.JsonSerializerSettings);
            return this;
        }
        public async Task<ModAction> InitAsync(Reddit reddit, JToken post, IWebAgent webAgent)
        {
            CommonInit(reddit, post, webAgent);
            await Task.Factory.StartNew(() => JsonConvert.PopulateObject(post["data"].ToString(), this, reddit.JsonSerializerSettings));
            return this;
        }

        private void CommonInit(Reddit reddit, JToken json, IWebAgent webAgent)
        {
            base.Init(json);
            Reddit = reddit;
            WebAgent = webAgent;
        }

    }
}
