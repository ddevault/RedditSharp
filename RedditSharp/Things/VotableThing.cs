using System.Threading.Tasks;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace RedditSharp.Things
{
    public class VotableThing : CreatedThing
    {
        public enum VoteType
        {
            Upvote = 1,
            None = 0,
            Downvote = -1
        }

        public enum ReportType
        {
            Spam = 0,
            VoteMinipulation = 1,
            PersonalInformation = 2,
            SexualizingMinors = 3,
            BreakingReddit = 4,
            Other = 5
        }

        private const string VoteUrl = "/api/vote";
        private const string SaveUrl = "/api/save";
        private const string UnsaveUrl = "/api/unsave";
        private const string ReportUrl = "/api/report";

        [JsonIgnore]
        private IWebAgent WebAgent { get; set; }

        [JsonIgnore]
        private Reddit Reddit { get; set; }

        protected VotableThing Init(Reddit reddit, IWebAgent webAgent, JToken json)
        {
            CommonInit(reddit, webAgent, json);
            JsonConvert.PopulateObject(json["data"].ToString(), this, Reddit.JsonSerializerSettings);
            return this;
        }
        protected async Task<VotableThing> InitAsync(Reddit reddit, IWebAgent webAgent, JToken json)
        {
            CommonInit(reddit, webAgent, json);
            await Task.Factory.StartNew(() => JsonConvert.PopulateObject(json["data"].ToString(), this, Reddit.JsonSerializerSettings));
            return this;
        }

        private void CommonInit(Reddit reddit, IWebAgent webAgent, JToken json)
        {
            base.Init(reddit, json);
            Reddit = reddit;
            WebAgent = webAgent;
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

        /// <summary>
        /// Gets or sets the vote for the current VotableThing.
        /// </summary>
        [JsonIgnore]
        public VoteType Vote
        {
            get
            {
                switch (this.Liked)
                {
                    case true: return VoteType.Upvote;
                    case false: return VoteType.Downvote;

                    default: return VoteType.None;
                }
            }
            set { this.SetVote(value); }
        }

        public void Upvote()
        {
            this.SetVote(VoteType.Upvote);
        }

        public void Downvote()
        {
            this.SetVote(VoteType.Downvote);
        }

        public void SetVote(VoteType type)
        {
            if (this.Vote == type) return;

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

            if (Liked == true) Upvotes--;
            if (Liked == false) Downvotes--;

            switch(type)
            {
                case VoteType.Upvote: Liked = true; Upvotes++; return;
                case VoteType.None: Liked = null; return;
                case VoteType.Downvote: Liked = false; Downvotes++; return;
            }
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

        public void Report(ReportType reportType)
        {
            Report(reportType, "");
        }

        public void Report(ReportType reportType, string otherReason)
        {
            var request = WebAgent.CreatePost(ReportUrl);
            var stream = request.GetRequestStream();

            string reportReason;
            switch (reportType)
            {
                case ReportType.Spam: 
                    reportReason = "spam"; break;
                case ReportType.VoteMinipulation: 
                    reportReason = "vote minipulation"; break;
                case ReportType.PersonalInformation: 
                    reportReason = "personal information"; break;
                case ReportType.BreakingReddit: 
                    reportReason = "breaking reddit"; break;
                case ReportType.SexualizingMinors: 
                    reportReason = "sexualizing minors"; break;
                default: 
                    reportReason = "other"; break;
            }

            WebAgent.WritePostBody(stream, new
            {
                api_type = "json",
                reason = reportReason,
                other_reason = otherReason ?? "",
                thing_id = FullName,
                uh = Reddit.User.Modhash
            });
            stream.Close();
            var response = request.GetResponse();
            var data = WebAgent.GetResponseString(response.GetResponseStream());
        }

    }
}
