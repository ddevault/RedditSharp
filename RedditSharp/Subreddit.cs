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
        private const string SubredditTopUrl = "/r/{0}/top.json?t={1}";
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
        private const string AddModeratorUrl = "/api/friend";
        private const string AddContributorUrl = "/api/friend";
        private const string ModeratorsUrl = "/r/{0}/about/moderators.json";
        private const string FrontPageUrl = "/.json";
        private const string SubmitLinkUrl = "/api/submit";
        private const string FlairListUrl = "/r/{0}/api/flairlist.json";
        private const string CommentsUrl = "/r/{0}/comments.json";

        [JsonIgnore]
        private Reddit Reddit { get; set; }

        [JsonIgnore]
        private IWebAgent WebAgent { get; set; }

        [JsonIgnore]
        public Wiki Wiki { get; private set; }

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
        [JsonConverter(typeof(UrlParser))]
        public Uri Url { get; set; }

        [JsonIgnore]
        public string Name { get; set; }

        public Listing<Post> GetTop(FromTime timePeriod)
        {
            if (Name == "/")
            {
                return new Listing<Post>(Reddit, "/top.json?t=" + Enum.GetName(typeof(FromTime), timePeriod).ToLower(), WebAgent);
            }
            return new Listing<Post>(Reddit, string.Format(SubredditTopUrl, Name, Enum.GetName(typeof(FromTime), timePeriod)).ToLower(), WebAgent);
        }

        public Listing<Post> Posts
        {
            get
            {
                if (Name == "/")
                    return new Listing<Post>(Reddit, "/.json", WebAgent);
                return new Listing<Post>(Reddit, string.Format(SubredditPostUrl, Name), WebAgent);
            }
        }

        public Listing<Comment> Comments
        {
            get
            {
                if (Name == "/")
                    return new Listing<Comment>(Reddit, "/comments.json", WebAgent);
                return new Listing<Comment>(Reddit, string.Format(CommentsUrl, Name), WebAgent);
            }
        }

        public Listing<Post> New
        {
            get
            {
                if (Name == "/")
                    return new Listing<Post>(Reddit, "/new.json", WebAgent);
                return new Listing<Post>(Reddit, string.Format(SubredditNewUrl, Name), WebAgent);
            }
        }

        public Listing<Post> Hot
        {
            get
            {
                if (Name == "/")
                    return new Listing<Post>(Reddit, "/.json", WebAgent);
                return new Listing<Post>(Reddit, string.Format(SubredditHotUrl, Name), WebAgent);
            }
        }

        public Listing<VotableThing> ModQueue
        {
            get
            {
                return new Listing<VotableThing>(Reddit, string.Format(ModqueueUrl, Name), WebAgent);
            }
        }

        public Listing<Post> UnmoderatedLinks
        {
            get
            {
                return new Listing<Post>(Reddit, string.Format(UnmoderatedUrl, Name), WebAgent);
            }
        }

        public SubredditSettings Settings
        {
            get
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
        }

        public UserFlairTemplate[] UserFlairTemplates // Hacky, there isn't a proper endpoint for this
        {
            get
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
        }

        public SubredditStyle Stylesheet
        {
            get
            {
                var request = WebAgent.CreateGet(string.Format(StylesheetUrl, Name));
                var response = request.GetResponse();
                var data = WebAgent.GetResponseString(response.GetResponseStream());
                var json = JToken.Parse(data);
                return new SubredditStyle(Reddit, this, json, WebAgent);
            }
        }

        public IEnumerable<ModeratorUser> Moderators
        {
            get
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
        }

        /// <summary>
        /// This constructor only exists for internal use and serialization.
        /// You would be wise not to use it.
        /// </summary>
        public Subreddit()
            : base(null)
        {
        }

        protected internal Subreddit(Reddit reddit, JToken json, IWebAgent webAgent)
            : base(json)
        {
            Reddit = reddit;
            WebAgent = webAgent;
            Wiki = new Wiki(reddit, this, webAgent);
            JsonConvert.PopulateObject(json["data"].ToString(), this, reddit.JsonSerializerSettings);
            Name = Url.ToString();
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
                Url = new Uri("/r/all", UriKind.Relative),
                Name = "all",
                Reddit = reddit,
                WebAgent = reddit._webAgent
            };
            return rSlashAll;
        }

        public static Subreddit GetFrontPage(Reddit reddit)
        {
            var frontPage = new Subreddit
            {
                DisplayName = "Front Page",
                Title = "reddit: the front page of the internet",
                Url = new Uri("/", UriKind.Relative),
                Name = "/",
                Reddit = reddit,
                WebAgent = reddit._webAgent
            };
            return frontPage;
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

        public string GetFlairText(string user)
        {
            var request = WebAgent.CreateGet(String.Format(FlairListUrl + "?name=" + user, Name));
            var response = request.GetResponse();
            var data = WebAgent.GetResponseString(response.GetResponseStream());
            var json = JToken.Parse(data);
            return (string)json["users"][0]["flair_text"];
        }

        public string GetFlairCssClass(string user)
        {
            var request = WebAgent.CreateGet(String.Format(FlairListUrl + "?name=" + user, Name));
            var response = request.GetResponse();
            var data = WebAgent.GetResponseString(response.GetResponseStream());
            var json = JToken.Parse(data);
            return (string)json["users"][0]["flair_css_class"];
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

        public void AddModerator(string user)
        {
            var request = WebAgent.CreatePost(AddModeratorUrl);
            WebAgent.WritePostBody(request.GetRequestStream(), new
            {
                api_type = "json",
                uh = Reddit.User.Modhash,
                r = Name,
                type = "moderator",
                name = user
            });
            var response = request.GetResponse();
            var result = WebAgent.GetResponseString(response.GetResponseStream());
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

        public override string ToString()
        {
            return "/r/" + DisplayName;
        }

        public void AddContributor(string user)
        {
            var request = WebAgent.CreatePost(AddContributorUrl);
            WebAgent.WritePostBody(request.GetRequestStream(), new
            {
                api_type = "json",
                uh = Reddit.User.Modhash,
                r = Name,
                type = "contributor",
                name = user
            });
            var response = request.GetResponse();
            var result = WebAgent.GetResponseString(response.GetResponseStream());
        }

        public void RemoveContributor(string id)
        {
            var request = WebAgent.CreatePost(LeaveModerationUrl);
            WebAgent.WritePostBody(request.GetRequestStream(), new
            {
                api_type = "json",
                uh = Reddit.User.Modhash,
                r = Name,
                type = "contributor",
                id
            });
            var response = request.GetResponse();
            var result = WebAgent.GetResponseString(response.GetResponseStream());
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

        private Post Submit(SubmitData data)
        {
            if (Reddit.User == null)
                throw new RedditException("No user logged in.");
            var request = WebAgent.CreatePost(SubmitLinkUrl);

            WebAgent.WritePostBody(request.GetRequestStream(), data);

            var response = request.GetResponse();
            var result = WebAgent.GetResponseString(response.GetResponseStream());
            var json = JToken.Parse(result);

            ICaptchaSolver solver = Reddit.CaptchaSolver;
            if (json["json"]["errors"].Any() && json["json"]["errors"][0][0].ToString() == "BAD_CAPTCHA"
                && solver != null)
            {
                data.Iden = json["json"]["captcha"].ToString();
                CaptchaResponse captchaResponse = solver.HandleCaptcha(new Captcha(data.Iden));

                // We throw exception due to this method being expected to return a valid Post object, but we cannot
                // if we got a Captcha error.
                if (captchaResponse.Cancel)
                    throw new CaptchaFailedException("Captcha verification failed when submitting " + data.Kind + " post");

                data.Captcha = captchaResponse.Answer;
                return Submit(data);
            }
            else if (json["json"]["errors"].Any() && json["json"]["errors"][0][0].ToString() == "ALREADY_SUB")
            {
                throw new DuplicateLinkException(String.Format("Post failed when submitting.  The following link has already been submitted: {0}", SubmitLinkUrl));
            }

            return new Post(Reddit, json["json"], WebAgent);
        }

        /// <summary>
        /// Submits a link post in the current subreddit using the logged-in user
        /// </summary>
        /// <param name="title">The title of the submission</param>
        /// <param name="url">The url of the submission link</param>
        public Post SubmitPost(string title, string url, string captchaId = "", string captchaAnswer = "", bool resubmit = false)
        {
            return
                Submit(
                    new LinkData
                    {
                        Subreddit = Name,
                        UserHash = Reddit.User.Modhash,
                        Title = title,
                        URL = url,
                        Resubmit = resubmit,
                        Iden = captchaId,
                        Captcha = captchaAnswer
                    });
        }

        /// <summary>
        /// Submits a text post in the current subreddit using the logged-in user
        /// </summary>
        /// <param name="title">The title of the submission</param>
        /// <param name="text">The raw markdown text of the submission</param>
        public Post SubmitTextPost(string title, string text, string captchaId = "", string captchaAnswer = "")
        {
            return
                Submit(
                    new TextData
                    {
                        Subreddit = Name,
                        UserHash = Reddit.User.Modhash,
                        Title = title,
                        Text = text,
                        Iden = captchaId,
                        Captcha = captchaAnswer
                    });
        }

        #region Obsolete Getter Methods

        [Obsolete("Use Posts property instead")]
        public Listing<Post> GetPosts()
        {
            return Posts;
        }

        [Obsolete("Use New property instead")]
        public Listing<Post> GetNew()
        {
            return New;
        }

        [Obsolete("Use Hot property instead")]
        public Listing<Post> GetHot()
        {
            return Hot;
        }

        [Obsolete("Use ModQueue property instead")]
        public Listing<VotableThing> GetModQueue()
        {
            return ModQueue;
        }

        [Obsolete("Use UnmoderatedLinks property instead")]
        public Listing<Post> GetUnmoderatedLinks()
        {
            return UnmoderatedLinks;
        }

        [Obsolete("Use Settings property instead")]
        public SubredditSettings GetSettings()
        {
            return Settings;
        }

        [Obsolete("Use UserFlairTemplates property instead")]
        public UserFlairTemplate[] GetUserFlairTemplates() // Hacky, there isn't a proper endpoint for this
        {
            return UserFlairTemplates;
        }

        [Obsolete("Use Stylesheet property instead")]
        public SubredditStyle GetStylesheet()
        {
            return Stylesheet;
        }

        [Obsolete("Use Moderators property instead")]
        public IEnumerable<ModeratorUser> GetModerators()
        {
            return Moderators;
        }

        #endregion Obsolete Getter Methods
    }
}
