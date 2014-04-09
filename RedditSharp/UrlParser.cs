﻿using Newtonsoft.Json;
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
            return new Uri(token.Value<string>(), UriKind.RelativeOrAbsolute);
        }

        public override void WriteJson(JsonWriter writer, object value, JsonSerializer serializer)
        {
            writer.WriteValue(value);
        }
    }
}
