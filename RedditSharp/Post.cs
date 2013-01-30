﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Authentication;
using System.Text;
using System.Web;
using Newtonsoft.Json.Linq;

namespace RedditSharp
{
    public class Post : VotableThing
    {
        private const string CommentUrl = "http://www.reddit.com/api/comment";
        private const string RemoveUrl = "http://www.reddit.com/api/remove";
        private const string GetCommentsUrl = "http://www.reddit.com/comments/{0}.json";

        private Reddit Reddit { get; set; }

        public Post(Reddit reddit, JToken post)
            : base(reddit, post)
        {
            Reddit = reddit;

            var data = post["data"];
            AuthorName = data["author"].Value<string>();
            AuthorFlairClass = data["author_flair_css_class"].Value<string>();
            AuthorFlairText = data["author_flair_text"].Value<string>();
            Created = Reddit.UnixTimeStampToDateTime(data["created"].Value<double>());
            Domain = data["domain"].Value<string>();
            Downvotes = data["downs"].Value<int>();
            Edited = data["edited"].Value<bool>();
            IsSelfPost = data["is_self"].Value<bool>();
            LinkFlairClass = data["link_flair_css_class"].Value<string>();
            LinkFlairText = data["link_flair_text"].Value<string>();
            CommentCount = data["num_comments"].Value<int>();
            NSFW = data["over_18"].Value<bool>();
            Permalink = data["permalink"].Value<string>();
            Saved = data["saved"].Value<bool>();
            Score = data["score"].Value<int>();
            SelfText = data["selftext"].Value<string>();
            SelfTextHtml = data["selftext_html"].Value<string>();
            Subreddit = data["subreddit"].Value<string>();
            Thumbnail = data["thumbnail"].Value<string>();
            Title = HttpUtility.HtmlDecode(data["title"].Value<string>());
            Upvotes = data["ups"].Value<int>();
            Url = data["url"].Value<string>();
            Name = data["name"].Value<string>().Replace("t3_", "");
        }

        public string AuthorName { get; set; }
        public RedditUser Author
        {
            get
            {
                return Reddit.GetUser(AuthorName);
            }
        }

        public string AuthorFlairClass { get; set; }
        public string AuthorFlairText { get; set; }
        public DateTime Created { get; set; }
        public string Domain { get; set; }
        public int Downvotes { get; set; }
        public bool Edited { get; set; }
        public bool IsSelfPost { get; set; }
        public string LinkFlairClass { get; set; }
        public string LinkFlairText { get; set; }
        public int CommentCount { get; set; }
        public bool NSFW { get; set; }
        public string Permalink { get; set; }
        public bool Saved { get; set; }
        public int Score { get; set; }
        public string SelfText { get; set; }
        public string SelfTextHtml { get; set; }
        public string Subreddit { get; set; }
        public string Thumbnail { get; set; }
        public string Title { get; set; }
        public int Upvotes { get; set; }
        public string Url { get; set; }
        public string Name { get; set; }

        public Comment Comment(string message)
        {
            if (Reddit.User == null)
                throw new AuthenticationException("No user logged in.");
            var request = Reddit.CreatePost(CommentUrl);
            var stream = request.GetRequestStream();
            Reddit.WritePostBody(stream, new
                {
                    text = message,
                    thing_id = FullName,
                    uh = Reddit.User.Modhash
                });
            stream.Close();
            var response = request.GetResponse();
            var data = Reddit.GetResponseString(response.GetResponseStream());
            var json = JObject.Parse(data);
            var comment = json["jquery"].FirstOrDefault(i => i[0].Value<int>() == 18 && i[1].Value<int>() == 19);
            return new Comment(Reddit, comment[3][0][0]);
        }

        public void Remove()
        {
            var request = Reddit.CreatePost(RemoveUrl);
            var stream = request.GetRequestStream();
            Reddit.WritePostBody(stream, new
            {
                id = FullName,
                spam = false,
                uh = Reddit.User.Modhash
            });
            stream.Close();
            var response = request.GetResponse();
            var data = Reddit.GetResponseString(response.GetResponseStream());
        }

        public void RemoveSpam()
        {
            var request = Reddit.CreatePost(RemoveUrl);
            var stream = request.GetRequestStream();
            Reddit.WritePostBody(stream, new
            {
                id = FullName,
                spam = true,
                uh = Reddit.User.Modhash
            });
            stream.Close();
            var response = request.GetResponse();
            var data = Reddit.GetResponseString(response.GetResponseStream());
        }

        public List<Comment> GetComments()
        {
            var comments = new List<Comment>();

            var request = Reddit.CreateGet(string.Format(GetCommentsUrl, Name));
            var response = request.GetResponse();
            var data = Reddit.GetResponseString(response.GetResponseStream());
            var json = JArray.Parse(data);

            var postJson = json.Last()["data"]["children"];
            foreach (var comment in postJson)
                comments.Add(new Comment(Reddit, comment));

            return comments;
        }

    }
}
