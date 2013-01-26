using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Authentication;
using System.Text;
using Newtonsoft.Json.Linq;

namespace RedditSharp
{
    public class Subreddit
    {
        private const string SubredditPostUrl = "http://www.reddit.com/r/{0}.json";
        private const string SubredditNewUrl = "http://www.reddit.com/r/{0}/new.json?sort=new";
        private const string SubscribeUrl = "http://www.reddit.com/api/subscribe";
        private const string GetSettingsUrl = "http://www.reddit.com/r/{0}/about/edit.json";
        private const string ModqueueUrl = "http://www.reddit.com/r/{0}/about/modqueue.json";
        private const string FlairTemplateUrl = "http://www.reddit.com/api/flairtemplate";
        private const string ClearFlairTemplatesUrl = "http://www.reddit.com/api/clearflairtemplates";
        private const string SetUserFlairUrl = "http://www.reddit.com/api/flair";

        private Reddit Reddit { get; set; }

        public DateTime Created { get; set; }
        public string Description { get; set; }
        public string DisplayName { get; set; }
        public string HeaderImage { get; set; }
        public string HeaderTitle { get; set; }
        public string Id { get; set; }
        public bool NSFW { get; set; }
        public string PublicDescription { get; set; }
        public int Subscribers { get; set; }
        public int? ActiveUsers { get; set; }
        public string Title { get; set; }
        public string Url { get; set; }
        public string Name { get; set; }

        private Subreddit()
        {
        }

        protected internal Subreddit(Reddit reddit, JToken json)
        {
            Reddit = reddit;
            var data = json["data"];
            Created = Reddit.UnixTimeStampToDateTime(data["created"].Value<double>());
            Description = data["description"].Value<string>();
            DisplayName = data["display_name"].Value<string>();
            HeaderImage = data["header_img"].Value<string>();
            HeaderTitle = data["header_title"].Value<string>();
            Id = data["name"].Value<string>();
            NSFW = data["over18"].Value<bool>();
            PublicDescription = data["public_description"].Value<string>();
            Subscribers = data["subscribers"].Value<int>();
            Title = data["title"].Value<string>();
            Url = data["url"].Value<string>();
            ActiveUsers = data["accounts_active"].Value<int?>();
            Name = Url;
            if (Name.StartsWith("/r/"))
                Name = Name.Substring(3);
            if (Name.StartsWith("r/"))
                Name = Name.Substring(2);
            Name = Name.TrimEnd('/');
        }

        public static Subreddit GetRSlashAll(Reddit reddit)
        {
            var rSlashAll = new Subreddit
            {
                DisplayName = "/r/all",
                Title = "/r/all",
                Url = "/r/all",
                Name = "all",
                Reddit = reddit
            };
            return rSlashAll;
        }

        public Post[] GetPosts()
        {
            var request = Reddit.CreateGet(string.Format(SubredditPostUrl, Name));
            var response = request.GetResponse();
            var data = Reddit.GetResponseString(response.GetResponseStream());
            var json = JObject.Parse(data);
            var posts = new List<Post>();
            var postJson = json["data"]["children"];
            foreach (var post in postJson)
                posts.Add(new Post(Reddit, post));
            return posts.ToArray();
        }

        public Post[] GetNew()
        {
            var request = Reddit.CreateGet(string.Format(SubredditNewUrl, Name));
            var response = request.GetResponse();
            var data = Reddit.GetResponseString(response.GetResponseStream());
            var json = JObject.Parse(data);
            var posts = new List<Post>();
            var postJson = json["data"]["children"];
            foreach (var post in postJson)
                posts.Add(new Post(Reddit, post));
            return posts.ToArray();
        }

        public void Subscribe()
        {
            if (Reddit.User == null)
                throw new AuthenticationException("No user logged in.");
            var request = Reddit.CreatePost(SubscribeUrl);
            var stream = request.GetRequestStream();
            Reddit.WritePostBody(stream, new
                {
                    action = "sub",
                    sr = Id,
                    uh = Reddit.User.Modhash
                });
            stream.Close();
            var response = request.GetResponse();
            var data = Reddit.GetResponseString(response.GetResponseStream());
            // Discard results
        }

        public void Unsubscribe()
        {
            if (Reddit.User == null)
                throw new AuthenticationException("No user logged in.");
            var request = Reddit.CreatePost(SubscribeUrl);
            var stream = request.GetRequestStream();
            Reddit.WritePostBody(stream, new
            {
                action = "unsub",
                sr = Id,
                uh = Reddit.User.Modhash
            });
            stream.Close();
            var response = request.GetResponse();
            var data = Reddit.GetResponseString(response.GetResponseStream());
            // Discard results
        }

        public SubredditSettings GetSettings()
        {
            if (Reddit.User == null)
                throw new AuthenticationException("No user logged in.");
            var request = Reddit.CreateGet(string.Format(GetSettingsUrl, Name));
            var response = request.GetResponse();
            var data = Reddit.GetResponseString(response.GetResponseStream());
            var json = JObject.Parse(data);
            return new SubredditSettings(this, Reddit, json);
        }

        public void ClearFlairTemplates(FlairType flairType)
        {
            var request = Reddit.CreatePost(ClearFlairTemplatesUrl);
            var stream = request.GetRequestStream();
            Reddit.WritePostBody(stream, new
            {
                flair_type = flairType == FlairType.Link ? "LINK_FLAIR" : "USER_FLAIR",
                uh = Reddit.User.Modhash,
                r = this.Name
            });
            stream.Close();
            var response = request.GetResponse();
            var data = Reddit.GetResponseString(response.GetResponseStream());
        }

        public void AddFlairTemplate(string cssClass, FlairType flairType, string text, bool userEditable)
        {
            var request = Reddit.CreatePost(FlairTemplateUrl);
            var stream = request.GetRequestStream();
            Reddit.WritePostBody(stream, new
            {
                css_class = cssClass,
                flair_type = flairType == FlairType.Link ? "LINK_FLAIR" : "USER_FLAIR",
                text = text,
                text_editable = userEditable,
                uh = Reddit.User.Modhash,
                r = this.Name
            });
            stream.Close();
            var response = request.GetResponse();
            var data = Reddit.GetResponseString(response.GetResponseStream());
            var json = JToken.Parse(data);
            if (json["jquery"].Count(j => j[0].Value<int>() == 14 && j[1].Value<int>() == 15) != 0)
                throw new OutOfMemoryException("Maximum flair templates reached");
        }

        public void SetUserFlair(string user, string cssClass, string text)
        {
            var request = Reddit.CreatePost(SetUserFlairUrl);
            var stream = request.GetRequestStream();
            Reddit.WritePostBody(stream, new
            {
                css_class = cssClass,
                text = text,
                uh = Reddit.User.Modhash,
                r = this.Name
            });
            stream.Close();
            var response = request.GetResponse();
            var data = Reddit.GetResponseString(response.GetResponseStream());
        }

        public override string ToString()
        {
            return "/r/" + DisplayName;
        }
    }
}
