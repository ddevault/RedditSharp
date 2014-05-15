using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;

namespace RedditSharp
{
    class UrlParser : JsonConverter
    {
        public override bool CanConvert(Type objectType)
        {
            return objectType == typeof(String) || objectType == typeof(Uri);
        }

        public override object ReadJson(JsonReader reader, Type objectType, object existingValue, JsonSerializer serializer)
        {
            var token = JToken.Load(reader);

            if (token.Type == JTokenType.String)
            {
                if (Type.GetType("Mono.Runtime") == null)
                    return new Uri(token.Value<string>(), UriKind.RelativeOrAbsolute);
                if (token.Value<string>().StartsWith("/"))
                    return new Uri(token.Value<string>(), UriKind.Relative);
                return new Uri(token.Value<string>(), UriKind.RelativeOrAbsolute);
            }
            else
                return token.Value<Uri>();
        }

        public override void WriteJson(JsonWriter writer, object value, JsonSerializer serializer)
        {
            writer.WriteValue(value);
        }
    }
}
