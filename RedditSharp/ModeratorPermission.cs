using System;
using System.Linq;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace RedditSharp
{
    [Flags]
    public enum ModeratorPermission
    {
        None   = 0x00,
        Access = 0x01,
        Config = 0x02,
        Flair  = 0x04,
        Mail   = 0x08,
        Posts  = 0x10,
        Wiki   = 0x20,
        All    = Access | Config | Flair | Mail | Posts | Wiki
    }

    internal class ModeratorPermissionConverter : JsonConverter
    {
        public override void WriteJson(JsonWriter writer, object value, JsonSerializer serializer)
        {
            throw new NotImplementedException();
        }

        public override object ReadJson(JsonReader reader, Type objectType, object existingValue, JsonSerializer serializer)
        {
            var data = String.Join(",", JArray.Load(reader).Select(t => t.ToString()));

            ModeratorPermission result;

            var valid = Enum.TryParse(data, true, out result);

            if (!valid)
                result = ModeratorPermission.None;

            return result;
        }

        public override bool CanConvert(Type objectType)
        {
            // NOTE: Not sure if this is what is supposed to be returned
            // This method wasn't called in my (Sharparam) tests so unsure what it does
            return objectType == typeof (ModeratorPermission);
        }
    }
}
