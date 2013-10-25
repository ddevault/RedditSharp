using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Authentication;
using HtmlAgilityPack;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace RedditSharp
{
    public class Subreddit : Thing
    {
        private const string SubredditPostUrl = "/r/{0}.json";
        private const string SubredditNewUrl = "/r/{0}/new.json?sort=new";
        private const string SubredditHotUrl = "/r/{0}/hot.json";
        private const string SubscribeUrl = "/api/subscribe";
        private const string GetSettingsUrl = "/r/{0}/about/edit.json";
        private const string GetReducedSettingsUrl = "/r/{0}/about.json";
        private const string ModqueueUrl = "/r/{0}/about/modqueue.json";
        private const string UnmoderatedUrl = "/r/{0}/about/unmoderated.json";
        private const string FlairTemplateUrl = "/api/flairtemplate";
        private const string ClearFlairTemplatesUrl = "/api/clearflairtemplates";
        private const string SetUserFlairUrl = "/api/flair";
        private const string StylesheetUrl = "/r/{0}/about/stylesheet.json";
        private const string UploadImageUrl = "/api/upload_sr_img";
        private const string FlairSelectorUrl = "/api/flairselector";
        private const string AcceptModeratorInviteUrl = "/api/accept_moderator_invite";
        private const string LeaveModerationUrl = "/api/unfriend";
        private const string BanUserUrl = "/api/friend";
        private const string ModeratorsUrl = "/r/{0}/about/moderators.json";
        private const string FrontPageUrl = "/.json";
        private const string SubmitLinkUrl = "/api/submit";

        [JsonIgnore]
        private Reddit Reddit { get; set; }

        [JsonIgnore]
        private IWebAgent WebAgent { get; set; }

        [JsonProperty("created")]
        [JsonConverter(typeof(UnixTimestampConverter))]
        public DateTime? Created { get; set; }
        [JsonProperty("description")]
        public string Description { get; set; }
        [JsonProperty("description_html")]
        public string DescriptionHTML { get; set; }
        [JsonProperty("display_name")]
        public string DisplayName { get; set; }
        [JsonProperty("header_img")]
        public string HeaderImage { get; set; }
        [JsonProperty("header_title")]
        public string HeaderTitle { get; set; }
        [JsonProperty("over18")]
        public bool? NSFW { get; set; }
        [JsonProperty("public_description")]
        public string PublicDescription { get; set; }
        [JsonProperty("subscribers")]
        public int? Subscribers { get; set; }
        [JsonProperty("accounts_active")]
        public int? ActiveUsers { get; set; }
        [JsonProperty("title")]
        public string Title { get; set; }
        [JsonProperty("url")]
        public string Url { get; set; }
        [JsonIgnore]
        public string Name { get; set; }

        /// <summary>
        /// This constructor only exists for internal use and serialization.
        /// You would be wise not to use it.
        /// </summary>
        public Subreddit() : base(null)
        {
        }

        protected internal Subreddit(Reddit reddit, JToken json, IWebAgent webAgent) : base(json)
        {
            Reddit = reddit;
            WebAgent = webAgent;
            JsonConvert.PopulateObject(json["data"].ToString(), this, reddit.JsonSerializerSettings);
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

        public static Subreddit GetFrontPage(Reddit reddit)
        {
            var frontPage = new Subreddit
            {
                DisplayName = "Front Page",
                Title = "reddit: the front page of the internet",
                Url = "/",
                Name = "/",
                Reddit = reddit
            };
            return frontPage;
        }

        public Listing<Post> GetPosts()
        {
            if (Name == "/")
                return new Listing<Post>(Reddit, "/.json", WebAgent);
            return new Listing<Post>(Reddit, string.Format(SubredditPostUrl, Name), WebAgent);
        }

        public Listing<Post> GetNew()
        {
            if (Name == "/")
                return new Listing<Post>(Reddit, "/new.json", WebAgent);
            return new Listing<Post>(Reddit, string.Format(SubredditNewUrl, Name), WebAgent);
        }

        public Listing<Post> GetHot()
        {
            if (Name == "/")
                return new Listing<Post>(Reddit, "/.json", WebAgent);
            return new Listing<Post>(Reddit, string.Format(SubredditHotUrl, Name), WebAgent);
        }

        public Listing<VotableThing> GetModQueue()
        {
            return new Listing<VotableThing>(Reddit, string.Format(ModqueueUrl, Name), WebAgent);
        }

        public Listing<Post> GetUnmoderatedLinks()
        {
            return new Listing<Post>(Reddit, string.Format(UnmoderatedUrl, Name), WebAgent);
        }

        public void Subscribe()
        {
            if (Reddit.User == null)
                throw new AuthenticationException("No user logged in.");
            var request = WebAgent.CreatePost(SubscribeUrl);
            var stream = request.GetRequestStream();
            WebAgent.WritePostBody(stream, new
                {
                    action = "sub",
                    sr = FullName,
                    uh = Reddit.User.Modhash
                });
            stream.Close();
            var response = request.GetResponse();
            var data = WebAgent.GetResponseString(response.GetResponseStream());
            // Discard results
        }

        public void Unsubscribe()
        {
            if (Reddit.User == null)
                throw new AuthenticationException("No user logged in.");
            var request = WebAgent.CreatePost(SubscribeUrl);
            var stream = request.GetRequestStream();
            WebAgent.WritePostBody(stream, new
            {
                action = "unsub",
                sr = FullName,
                uh = Reddit.User.Modhash
            });
            stream.Close();
            var response = request.GetResponse();
            var data = WebAgent.GetResponseString(response.GetResponseStream());
            // Discard results
        }

        public SubredditSettings GetSettings()
        {
            if (Reddit.User == null)
                throw new AuthenticationException("No user logged in.");
            try
            {
                var request = WebAgent.CreateGet(string.Format(GetSettingsUrl, Name));
                var response = request.GetResponse();
                var data = WebAgent.GetResponseString(response.GetResponseStream());
                var json = JObject.Parse(data);
                return new SubredditSettings(this, Reddit, json, WebAgent);
            }
            catch // TODO: More specific catch
            {
                // Do it unauthed
                var request = WebAgent.CreateGet(string.Format(GetReducedSettingsUrl, Name));
                var response = request.GetResponse();
                var data = WebAgent.GetResponseString(response.GetResponseStream());
                var json = JObject.Parse(data);
                return new SubredditSettings(this, Reddit, json, WebAgent);
            }
        }

        public void ClearFlairTemplates(FlairType flairType)
        {
            var request = WebAgent.CreatePost(ClearFlairTemplatesUrl);
            var stream = request.GetRequestStream();
            WebAgent.WritePostBody(stream, new
            {
                flair_type = flairType == FlairType.Link ? "LINK_FLAIR" : "USER_FLAIR",
                uh = Reddit.User.Modhash,
                r = Name
            });
            stream.Close();
            var response = request.GetResponse();
            var data = WebAgent.GetResponseString(response.GetResponseStream());
        }

        public void AddFlairTemplate(string cssClass, FlairType flairType, string text, bool userEditable)
        {
            var request = WebAgent.CreatePost(FlairTemplateUrl);
            var stream = request.GetRequestStream();
            WebAgent.WritePostBody(stream, new
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
            var data = WebAgent.GetResponseString(response.GetResponseStream());
            var json = JToken.Parse(data);
        }

        public void SetUserFlair(string user, string cssClass, string text)
        {
            var request = WebAgent.CreatePost(SetUserFlairUrl);
            var stream = request.GetRequestStream();
            WebAgent.WritePostBody(stream, new
            {
                css_class = cssClass,
                text = text,
                uh = Reddit.User.Modhash,
                r = Name,
                name = user
            });
            stream.Close();
            var response = request.GetResponse();
            var data = WebAgent.GetResponseString(response.GetResponseStream());
        }

        public UserFlairTemplate[] GetUserFlairTemplates() // Hacky, there isn't a proper endpoint for this
        {
            var request = WebAgent.CreatePost(FlairSelectorUrl);
            var stream = request.GetRequestStream();
            WebAgent.WritePostBody(stream, new
            {
                name = Reddit.User.Name,
                r = Name,
                uh = Reddit.User.Modhash
            });
            stream.Close();
            var response = request.GetResponse();
            var data = WebAgent.GetResponseString(response.GetResponseStream());
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
            var request = WebAgent.CreatePost(UploadImageUrl);
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
            var data = WebAgent.GetResponseString(response.GetResponseStream());
            // TODO: Detect errors
        }

        public SubredditStyle GetStylesheet()
        {
            var request = WebAgent.CreateGet(string.Format(StylesheetUrl, Name));
            var response = request.GetResponse();
            var data = WebAgent.GetResponseString(response.GetResponseStream());
            var json = JToken.Parse(data);
            return new SubredditStyle(Reddit, this, json, WebAgent);
        }

        public void AcceptModeratorInvite()
        {
            var request = WebAgent.CreatePost(AcceptModeratorInviteUrl);
            WebAgent.WritePostBody(request.GetRequestStream(), new
            {
                api_type = "json",
                uh = Reddit.User.Modhash,
                r = Name
            });
            var response = request.GetResponse();
            var result = WebAgent.GetResponseString(response.GetResponseStream());
        }

        public void RemoveModerator(string id)
        {
            var request = WebAgent.CreatePost(LeaveModerationUrl);
            WebAgent.WritePostBody(request.GetRequestStream(), new
            {
                api_type = "json",
                uh = Reddit.User.Modhash,
                r = Name,
                type = "moderator",
                id
            });
            var response = request.GetResponse();
            var result = WebAgent.GetResponseString(response.GetResponseStream());
        }

        public IEnumerable<ModeratorUser> GetModerators()
        {
            var request = WebAgent.CreateGet(string.Format(ModeratorsUrl, Name));
            var response = request.GetResponse();
            var responseString = WebAgent.GetResponseString(response.GetResponseStream());
            var json = JObject.Parse(responseString);
            var type = json["kind"].ToString();
            if (type != "UserList")
                throw new FormatException("Reddit responded with an object that is not a user listing.");
            var data = json["data"];
            var mods = data["children"].ToArray();
            var result = new ModeratorUser[mods.Length];
            for (var i = 0; i < mods.Length; i++)
            {
                var mod = new ModeratorUser(Reddit, mods[i]);
                result[i] = mod;
            }
            return result;
        }

        public override string ToString()
        {
            return "/r/" + DisplayName;
        }
        
        /// <summary>
        /// Submits a text post in the current subreddit using the logged-in user
        /// </summary>
        /// <param name="title">The title of the submission</param>
        /// <param name="text">The raw markdown text of the submission</param>
        public Post SubmitTextPost(string title, string text)
        {
            if (Reddit.User == null)
                throw new Exception("No user logged in.");
            var request = WebAgent.CreatePost(SubmitLinkUrl);

            WebAgent.WritePostBody(request.GetRequestStream(), new
            {
                api_type = "json",
                kind = "self",
                sr = Name,
                text = text,
                title = title,
                uh = Reddit.User.Modhash
            });
            var response = request.GetResponse();
            var result = WebAgent.GetResponseString(response.GetResponseStream());
            var json = JToken.Parse(result);
            return new Post(Reddit, json["json"], WebAgent);
            // TODO: Error
        }

        public void BanUser(string user, string reason)
        {
            var request = WebAgent.CreatePost(BanUserUrl);
            WebAgent.WritePostBody(request.GetRequestStream(), new
                                   {
                api_type = "json",
                uh = Reddit.User.Modhash,
                r = Name,
                type = "banned",
                id = "#banned",
                name = user,
                note = reason,
                action = "add",
                container = FullName
            });
            var response = request.GetResponse();
            var result = WebAgent.GetResponseString(response.GetResponseStream());
        }

        /// <summary>
        /// Submits a link post in the current subreddit using the logged-in user
        /// </summary>
        /// <param name="title">The title of the submission</param>
        /// <param name="url">The url of the submission link</param>
        public Post SubmitPost(string title, string url)
        {
            if (Reddit.User == null)
                throw new Exception("No user logged in.");
            var request = WebAgent.CreatePost(SubmitLinkUrl);

            WebAgent.WritePostBody(request.GetRequestStream(), new
            {
                api_type = "json",
                extension = "json",
                kind = "link",
                sr = Name,
                title = title,
                uh = Reddit.User.Modhash,
                url = url
            });
            var response = request.GetResponse();
            var result = WebAgent.GetResponseString(response.GetResponseStream());
            var json = JToken.Parse(result);
            return new Post(Reddit, json["json"], WebAgent);
            // TODO: Error
        }
    }
}
