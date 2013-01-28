using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Newtonsoft.Json.Linq;

namespace RedditSharp
{
    public class VotableThing : Thing
    {
        public enum VoteType
        {
            Upvote = 1,
            None = 0,
            Downvote = -1
        }

        private const string VoteUrl = "";

        private Reddit Reddit { get; set; }

        public VotableThing(Reddit reddit, JToken json) : base(json)
        {
            Reddit = reddit;
        }

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
        }
    }
}
