using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace RedditSharp
{
    public class ModeratorUser
    {
        public ModeratorUser(Reddit reddit, JToken json)
        {
            JsonConvert.PopulateObject(json.ToString(), this, reddit.JsonSerializerSettings);
        }

        [JsonProperty("name")]
        public string Name { get; set; }

        [JsonProperty("mod_permissions")]
        public string[] Permissions { get; set; }

        public override string ToString()
        {
            return Name;
        }
    }
}
