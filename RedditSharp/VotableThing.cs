using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Newtonsoft.Json.Linq;

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

        private Reddit Reddit { get; set; }

        public VotableThing(Reddit reddit, JToken json) : base(reddit, json)
        {
            Reddit = reddit;

            var data = json["data"];
            Upvotes = data["ups"].ValueOrDefault<int>();
            Downvotes = data["downs"].ValueOrDefault<int>();
            Liked = data["likes"].ValueOrDefault<bool?>();
        }

        public int Downvotes { get; set; }
        public int Upvotes { get; set; }
        /// <summary>
        /// True if the logged in user has upvoted this.
        /// False if they have not.
        /// Null if they have not cast a vote.
        /// </summary>
        public bool? Liked { get; set; }

        public void Upvote()
        {
            var request = Reddit.CreatePost(VoteUrl);
            var stream = request.GetRequestStream();
            Reddit.WritePostBody(stream, new
            {
                dir = 1,
                id = FullName,
                uh = Reddit.User.Modhash
            });
            stream.Close();
            var response = request.GetResponse();
            var data = Reddit.GetResponseString(response.GetResponseStream());
            Liked = true;
        }

        public void Downvote()
        {
            var request = Reddit.CreatePost(VoteUrl);
            var stream = request.GetRequestStream();
            Reddit.WritePostBody(stream, new
            {
                dir = -1,
                id = FullName,
                uh = Reddit.User.Modhash
            });
            stream.Close();
            var response = request.GetResponse();
            var data = Reddit.GetResponseString(response.GetResponseStream());
            Liked = false;
        }

        public void ClearVote()
        {
            var request = Reddit.CreatePost(VoteUrl);
            var stream = request.GetRequestStream();
            Reddit.WritePostBody(stream, new
            {
                dir = 0,
                id = FullName,
                uh = Reddit.User.Modhash
            });
            stream.Close();
            var response = request.GetResponse();
            var data = Reddit.GetResponseString(response.GetResponseStream());
        }

        public void Vote(VoteType type)
        {
            var request = Reddit.CreatePost(VoteUrl);
            var stream = request.GetRequestStream();
            Reddit.WritePostBody(stream, new
            {
                dir = (int)type,
                id = FullName,
                uh = Reddit.User.Modhash
            });
            stream.Close();
            var response = request.GetResponse();
            var data = Reddit.GetResponseString(response.GetResponseStream());
            Liked = null;
        }
    }
}
