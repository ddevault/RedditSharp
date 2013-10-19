using Newtonsoft.Json.Linq;
using Newtonsoft.Json;

namespace RedditSharp
{
    public class VotableThing : CreatedThing
    {
        public enum VoteType
        {
            Upvote = 1,
            None = 0,
            Downvote = -1
        }

        private const string VoteUrl = "/api/vote";
        private const string SaveUrl = "/api/save";
        private const string UnsaveUrl = "/api/unsave";

        [JsonIgnore]
        private IWebAgent WebAgent { get; set; }

        [JsonIgnore]
        private Reddit Reddit { get; set; }

        public VotableThing(Reddit reddit, IWebAgent webAgent, JToken json) : base(reddit, json)
        {
            Reddit = reddit;
            WebAgent = webAgent;
            JsonConvert.PopulateObject(json["data"].ToString(), this, Reddit.JsonSerializerSettings);
        }

        [JsonProperty("downs")]
        public int Downvotes { get; set; }
        [JsonProperty("ups")]
        public int Upvotes { get; set; }
        [JsonProperty("saved")]
        public bool Saved { get; set; }

        /// <summary>
        /// True if the logged in user has upvoted this.
        /// False if they have not.
        /// Null if they have not cast a vote.
        /// </summary>
        [JsonProperty("likes")]
        public bool? Liked { get; set; }

        public void Upvote()
        {
            var request = WebAgent.CreatePost(VoteUrl);
            var stream = request.GetRequestStream();
            WebAgent.WritePostBody(stream, new
            {
                dir = 1,
                id = FullName,
                uh = Reddit.User.Modhash
            });
            stream.Close();
            var response = request.GetResponse();
            var data = WebAgent.GetResponseString(response.GetResponseStream());
            Liked = true;
        }

        public void Downvote()
        {
            var request = WebAgent.CreatePost(VoteUrl);
            var stream = request.GetRequestStream();
            WebAgent.WritePostBody(stream, new
            {
                dir = -1,
                id = FullName,
                uh = Reddit.User.Modhash
            });
            stream.Close();
            var response = request.GetResponse();
            var data = WebAgent.GetResponseString(response.GetResponseStream());
            Liked = false;
        }

        public void Save()
        {
            var request = WebAgent.CreatePost(SaveUrl);
            var stream = request.GetRequestStream();
            WebAgent.WritePostBody(stream, new
            {
                id = FullName,
                uh = Reddit.User.Modhash
            });
            stream.Close();
            var response = request.GetResponse();
            var data = WebAgent.GetResponseString(response.GetResponseStream());
            Saved = true;
        }

        public void Unsave()
        {
            var request = WebAgent.CreatePost(UnsaveUrl);
            var stream = request.GetRequestStream();
            WebAgent.WritePostBody(stream, new
            {
                id = FullName,
                uh = Reddit.User.Modhash
            });
            stream.Close();
            var response = request.GetResponse();
            var data = WebAgent.GetResponseString(response.GetResponseStream());
            Saved = false;
        }

        public void ClearVote()
        {
            var request = WebAgent.CreatePost(VoteUrl);
            var stream = request.GetRequestStream();
            WebAgent.WritePostBody(stream, new
            {
                dir = 0,
                id = FullName,
                uh = Reddit.User.Modhash
            });
            stream.Close();
            var response = request.GetResponse();
            var data = WebAgent.GetResponseString(response.GetResponseStream());
        }

        public void Vote(VoteType type)
        {
            var request = WebAgent.CreatePost(VoteUrl);
            var stream = request.GetRequestStream();
            WebAgent.WritePostBody(stream, new
            {
                dir = (int)type,
                id = FullName,
                uh = Reddit.User.Modhash
            });
            stream.Close();
            var response = request.GetResponse();
            var data = WebAgent.GetResponseString(response.GetResponseStream());
            Liked = null;
        }
    }
}
