using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Authentication;
using System.Text;
using HtmlAgilityPack;
using Newtonsoft.Json.Linq;

namespace RedditSharp
{
    public class Subreddit : Thing
    {
        private const string SubredditPostUrl = "/r/{0}.json";
        private const string SubredditNewUrl = "/r/{0}/new.json?sort=new";
        private const string SubscribeUrl = "/api/subscribe";
        private const string GetSettingsUrl = "/r/{0}/about/edit.json";
        private const string GetReducedSettingsUrl = "/r/{0}/about.json";
        private const string ModqueueUrl = "/r/{0}/about/modqueue.json";
        private const string FlairTemplateUrl = "/api/flairtemplate";
        private const string ClearFlairTemplatesUrl = "/api/clearflairtemplates";
        private const string SetUserFlairUrl = "/api/flair";
        private const string StylesheetUrl = "/r/{0}/about/stylesheet.json";
        private const string UploadImageUrl = "/api/upload_sr_img";
        private const string FlairSelectorUrl = "/api/flairselector";

        private Reddit Reddit { get; set; }

        public DateTime Created { get; set; }
        public string Description { get; set; }
        public string DisplayName { get; set; }
        public string HeaderImage { get; set; }
        public string HeaderTitle { get; set; }
        public bool NSFW { get; set; }
        public string PublicDescription { get; set; }
        public int Subscribers { get; set; }
        public int? ActiveUsers { get; set; }
        public string Title { get; set; }
        public string Url { get; set; }
        public string Name { get; set; }

        /// <summary>
        /// This constructor only exists for internal use and serialization.
        /// You would be wise not to use it.
        /// </summary>
        public Subreddit() : base(null)
        {
        }

        protected internal Subreddit(Reddit reddit, JToken json) : base(json)
        {
            Reddit = reddit;
            var data = json["data"];
            Created = Reddit.UnixTimeStampToDateTime(data["created"].Value<double>());
            Description = data["description"].Value<string>();
            DisplayName = data["display_name"].Value<string>();
            HeaderImage = data["header_img"].Value<string>();
            HeaderTitle = data["header_title"].Value<string>();
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
                    sr = FullName,
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
                sr = FullName,
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
            try
            {
                var request = Reddit.CreateGet(string.Format(GetSettingsUrl, Name));
                var response = request.GetResponse();
                var data = Reddit.GetResponseString(response.GetResponseStream());
                var json = JObject.Parse(data);
                return new SubredditSettings(this, Reddit, json);
            }
            catch // TODO: More specific catch
            {
                // Do it unauthed
                var request = Reddit.CreateGet(string.Format(GetReducedSettingsUrl, Name));
                var response = request.GetResponse();
                var data = Reddit.GetResponseString(response.GetResponseStream());
                var json = JObject.Parse(data);
                return new SubredditSettings(this, Reddit, json);
            }
        }

        public void ClearFlairTemplates(FlairType flairType)
        {
            var request = Reddit.CreatePost(ClearFlairTemplatesUrl);
            var stream = request.GetRequestStream();
            Reddit.WritePostBody(stream, new
            {
                flair_type = flairType == FlairType.Link ? "LINK_FLAIR" : "USER_FLAIR",
                uh = Reddit.User.Modhash,
                r = Name
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
                r = Name,
                api_type = "json"
            });
            stream.Close();
            var response = request.GetResponse();
            var data = Reddit.GetResponseString(response.GetResponseStream());
            var json = JToken.Parse(data);
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
                r = Name,
                name = user
            });
            stream.Close();
            var response = request.GetResponse();
            var data = Reddit.GetResponseString(response.GetResponseStream());
        }

        public UserFlairTemplate[] GetUserFlairTemplates() // Hacky, there isn't a proper endpoint for this
        {
            var request = Reddit.CreatePost(FlairSelectorUrl);
            var stream = request.GetRequestStream();
            Reddit.WritePostBody(stream, new
            {
                name = Reddit.User.Name,
                r = Name,
                uh = Reddit.User.Modhash
            });
            stream.Close();
            var response = request.GetResponse();
            var data = Reddit.GetResponseString(response.GetResponseStream());
            var document = new HtmlDocument();
            document.LoadHtml(data);
            if (document.DocumentNode.Descendants("div").First().Attributes["error"] != null)
                throw new InvalidOperationException("This subreddit does not allow users to select flair.");
            var templateNodes = document.DocumentNode.Descendants("li");
            var list = new List<UserFlairTemplate>();
            foreach (var node in templateNodes)
            {
                list.Add(new UserFlairTemplate
                {
                    CssClass = node.Descendants("span").First().Attributes["class"].Value.Split(' ')[1],
                    Text = node.Descendants("span").First().InnerText
                });
            }
            return list.ToArray();
        }

        public void UploadHeaderImage(string name, ImageType imageType, byte[] file)
        {
            var request = Reddit.CreatePost(UploadImageUrl);
            var formData = new MultipartFormBuilder(request);
            formData.AddDynamic(new
            {
                name,
                uh = Reddit.User.Modhash,
                r = Name,
                formid = "image-upload",
                img_type = imageType == ImageType.PNG ? "png" : "jpg",
                upload = "",
                header = 1
            });
            formData.AddFile("file", "foo.png", file, imageType == ImageType.PNG ? "image/png" : "image/jpeg");
            formData.Finish();
            var response = request.GetResponse();
            var data = Reddit.GetResponseString(response.GetResponseStream());
            // TODO: Detect errors
        }

        public SubredditStyle GetStylesheet()
        {
            var request = Reddit.CreateGet(string.Format(StylesheetUrl, Name));
            var response = request.GetResponse();
            var data = Reddit.GetResponseString(response.GetResponseStream());
            var json = JToken.Parse(data);
            return new SubredditStyle(Reddit, this, json);
        }

        public override string ToString()
        {
            return "/r/" + DisplayName;
        }
    }
}
