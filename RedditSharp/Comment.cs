using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Authentication;
using System.Text;
using Newtonsoft.Json.Linq;
using System.Net;
using System.IO;
using Newtonsoft.Json;

namespace RedditSharp
{
    public class Comment : VotableThing
    {
        private const string CommentUrl = "/api/comment";
        private const string DistinguishUrl = "/api/distinguish";
        [JsonIgnore]
        private Reddit Reddit { get; set; }

        public Comment(Reddit reddit, JToken json)
            : base(reddit, json)
        {
            var data = json["data"];
            JsonConvert.PopulateObject(data.ToString(), this, reddit.JsonSerializerSettings);
            Reddit = reddit;

            //Parse sub comments
            Comments = new List<Comment>();
            if (data["replies"] != null && data["replies"].Any())
            {
                foreach (var comment in data["replies"]["data"]["children"])
                    Comments.Add(new Comment(reddit, comment));
            }
        }

        [JsonProperty("author")]
        public string Author { get; set; }
        [JsonProperty("body")]
        public string Content { get; set; }
        [JsonProperty("body_html")]
        public string ContentHtml { get; set; }
        [JsonProperty("parent_id")]
        public string ParentId { get; set; }
        [JsonProperty("subreddit")]
        public string Subreddit { get; set; }
        [JsonProperty("approved_by")]
        public string ApprovedBy { get; set; }
        [JsonProperty("author_flair_css_class")]
        public string AuthorFlairCssClass { get; set; }
        [JsonProperty("author_flair_text")]
        public string AuthorFlairText { get; set; }
        [JsonProperty("banned_by")]
        public string RemovedBy { get; set; }
        [JsonProperty("gilded")]
        public int Gilded { get; set; }
        [JsonProperty("link_id")]
        public string LinkId { get; set; }
        [JsonProperty("link_title")]
        public string LinkTitle { get; set; }
        [JsonProperty("num_reports")]
        public int NumReports { get; set; }
        [JsonProperty("distinguished")]
        [JsonConverter(typeof(DistinguishConverter))]
        public DistinguishType Distinguished { get; set; }

        public List<Comment> Comments { get; set; }

        public Comment Reply(string message)
        {
            if (Reddit.User == null)
                throw new AuthenticationException("No user logged in.");
            var request = Reddit.CreatePost(CommentUrl);
            var stream = request.GetRequestStream();
            Reddit.WritePostBody(stream, new
            {
                text = message,
                thing_id = FullName,
                uh = Reddit.User.Modhash,
                api_type = "json"
                //r = Subreddit
            });
            stream.Close();
            try
            {
                var response = request.GetResponse();
                var data = Reddit.GetResponseString(response.GetResponseStream());
                var json = JObject.Parse(data);
                return new Comment(Reddit, json["json"]["data"]["things"][0]);
            }
            catch (WebException ex)
            {
                var error = new StreamReader(ex.Response.GetResponseStream()).ReadToEnd();
                return null;
            }
        }

        public void Distinguish(DistinguishType distinguishType)
        {
            if (Reddit.User == null)
                throw new AuthenticationException("No user logged in.");
            var request = Reddit.CreatePost(DistinguishUrl);
            var stream = request.GetRequestStream();
            string how;
            switch (distinguishType)
            {
                case DistinguishType.Admin:
                    how = "admin";
                    break;
                case DistinguishType.Moderator:
                    how = "yes";
                    break;
                case DistinguishType.None:
                    how = "no";
                    break;
                default:
                    how = "special";
                    break;
            }
            Reddit.WritePostBody(stream, new
            {
                how,
                id = Id,
                uh = Reddit.User.Modhash
            });
            stream.Close();
            var response = request.GetResponse();
            var data = Reddit.GetResponseString(response.GetResponseStream());
            var json = JObject.Parse(data);
            if (json["jquery"].Count(i => i[0].Value<int>() == 11 && i[1].Value<int>() == 12) == 0)
                throw new AuthenticationException("You are not permitted to distinguish this comment.");
        }
    }

    public enum DistinguishType
    {
        Moderator,
        Admin,
        Special,
        None
    }

    internal class DistinguishConverter : JsonConverter
    {
        public override bool CanConvert(Type objectType)
        {
            return objectType == typeof(DistinguishType) || objectType == typeof(string);
        }

        public override object ReadJson(JsonReader reader, Type objectType, object existingValue, JsonSerializer serializer)
        {
            var token = JToken.Load(reader);
            var value = token.Value<string>();
            if (value == null)
                return DistinguishType.None;
            switch (value)
            {
                case "moderator": return DistinguishType.Moderator;
                case "admin": return DistinguishType.Admin;
                case "special": return DistinguishType.Special;
                default: return DistinguishType.None;
            }
        }

        public override void WriteJson(JsonWriter writer, object value, JsonSerializer serializer)
        {
            var d = (DistinguishType)value;
            if (d == DistinguishType.None)
            {
                writer.WriteNull();
                return;
            }
            writer.WriteValue(d.ToString().ToLower());
        }
    }

}
