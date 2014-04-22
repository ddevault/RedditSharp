using Newtonsoft.Json.Linq;
using System.Collections.Generic;
using System.Linq;

namespace RedditSharp
{
    using System;
    using System.Threading.Tasks;

    public class Wiki
    {
        private Reddit Reddit { get; set; }
        private Subreddit Subreddit { get; set; }
        private IAsyncWebAgent WebAgent { get; set; }

        private const string GetWikiPageUrl = "/r/{0}/wiki/{1}.json?v={2}";
        private const string GetWikiPagesUrl = "/r/{0}/wiki/pages.json";
        private const string WikiPageEditUrl = "/r/{0}/api/wiki/edit";
        private const string HideWikiPageUrl = "/r/{0}/api/wiki/hide";
        private const string RevertWikiPageUrl = "/r/{0}/api/wiki/revert";
        private const string WikiPageAllowEditorAddUrl = "/r/{0}/api/wiki/alloweditor/add";
        private const string WikiPageAllowEditorDelUrl = "/r/{0}/api/wiki/alloweditor/del";
        private const string WikiPageSettingsUrl = "/r/{0}/wiki/settings/{1}.json";
        private const string WikiRevisionsUrl = "/r/{0}/wiki/revisions.json";
        private const string WikiPageRevisionsUrl = "/r/{0}/wiki/revisions/{1}.json";
        private const string WikiPageDiscussionsUrl = "/r/{0}/wiki/discussions/{1}.json";

        public IEnumerable<string> PageNames
        {
            get
            {
                var request = WebAgent.CreateGet(string.Format(GetWikiPagesUrl, Subreddit.Name));
                var response = request.GetResponse();
                string json = WebAgent.GetResponseString(response.GetResponseStream());
                return JObject.Parse(json)["data"].Values<string>();
            }
        }

        public Task<IEnumerable<string>> PageNamesAsync
        {
            get
            {
                return GetPageNamesAsync();
            }
        }

        private async Task<IEnumerable<string>> GetPageNamesAsync()
        {
            var request = await WebAgent.CreateGetAsync(string.Format(GetWikiPagesUrl, Subreddit.Name));
            var response = await request.GetResponseAsync();
            string json = await WebAgent.GetResponseStringAsync(response.GetResponseStream());
            return JObject.Parse(json)["data"].Values<string>();
        }

        public Listing<WikiPageRevision> Revisions
        {
            get
            {
                return new Listing<WikiPageRevision>(Reddit, string.Format(WikiRevisionsUrl, Subreddit.Name), WebAgent);
            }
        }

        protected internal Wiki(Reddit reddit, Subreddit subreddit, IAsyncWebAgent webAgent)
        {
            Reddit = reddit;
            Subreddit = subreddit;
            WebAgent = webAgent;
        }

        public WikiPage GetPage(string page, string version = null)
        {
            var request = WebAgent.CreateGet(string.Format(GetWikiPageUrl, Subreddit.Name, page, version));
            var response = request.GetResponse();
            var json = JObject.Parse(WebAgent.GetResponseString(response.GetResponseStream()));
            var result = new WikiPage(Reddit, json["data"], WebAgent);
            return result;
        }

        public async Task<WikiPage> GetPageAsync(string page, string version = null)
        {
            var request = await WebAgent.CreateGetAsync(string.Format(GetWikiPageUrl, Subreddit.Name, page, version));
            var response = await request.GetResponseAsync();
            var data = await WebAgent.GetResponseStringAsync(response.GetResponseStream());
            var json = JObject.Parse(data);
            var result = new WikiPage(Reddit, json["data"], WebAgent);
            return result;
        }

        #region Settings
        public WikiPageSettings GetPageSettings(string name)
        {
            var request = WebAgent.CreateGet(string.Format(WikiPageSettingsUrl, Subreddit.Name, name));
            var response = request.GetResponse();
            var json = JObject.Parse(WebAgent.GetResponseString(response.GetResponseStream()));
            var result = new WikiPageSettings(Reddit, json["data"], WebAgent);
            return result;
        }

        public async Task<WikiPageSettings> GetPageSettingsAsync(string name)
        {
            var request = await WebAgent.CreateGetAsync(string.Format(WikiPageSettingsUrl, Subreddit.Name, name));
            var response = await request.GetResponseAsync();
            var data = await WebAgent.GetResponseStringAsync(response.GetResponseStream());
            var json = JObject.Parse(data);
            var result = new WikiPageSettings(Reddit, json["data"], WebAgent);
            return result;
        }

        public void SetPageSettings(string name, WikiPageSettings settings)
        {
            var request = WebAgent.CreatePost(string.Format(WikiPageSettingsUrl, Subreddit.Name, name));
            WebAgent.WritePostBody(request.GetRequestStream(), new
            {
                page = name,
                permlevel = settings.PermLevel,
                listed = settings.Listed,
                uh = Reddit.User.Modhash
            });
            var response = request.GetResponse();
        }

        public async Task SetPageSettingsAsync(string name, WikiPageSettings settings)
        {
            var request = await WebAgent.CreatePostAsync(string.Format(WikiPageSettingsUrl, Subreddit.Name, name));
            await WebAgent.WritePostBodyAsync(request.GetRequestStream(), new
            {
                page = name,
                permlevel = settings.PermLevel,
                listed = settings.Listed,
                uh = Reddit.User.Modhash
            });
            var response = await request.GetResponseAsync();
        }
        #endregion

        #region Revisions

        public Listing<WikiPageRevision> GetPageRevisions(string page)
        {
            return new Listing<WikiPageRevision>(Reddit, string.Format(WikiPageRevisionsUrl, Subreddit.Name, page), WebAgent);
        }
        #endregion

        #region Discussions
        public Listing<Post> GetPageDiscussions(string page)
        {
            return new Listing<Post>(Reddit, string.Format(WikiPageDiscussionsUrl, Subreddit.Name, page), WebAgent);
        }
        #endregion

        public void EditPage(string page, string content, string previous = null, string reason = null)
        {
            var request = WebAgent.CreatePost(string.Format(WikiPageEditUrl, Subreddit.Name));
            WebAgent.WritePostBody(request.GetRequestStream(), new
            {
                content = content,
                previous = previous,
                reason = reason,
                page = page,
                uh = Reddit.User.Modhash
            });
            var response = request.GetResponse();
        }

        public async Task EditPageAsync(string page, string content, string previous = null, string reason = null)
        {
            var request = await WebAgent.CreatePostAsync(string.Format(WikiPageEditUrl, Subreddit.Name));
            var stream = await request.GetRequestStreamAsync();
            await WebAgent.WritePostBodyAsync(stream, new
            {
                content = content,
                previous = previous,
                reason = reason,
                page = page,
                uh = Reddit.User.Modhash
            });
            var response = await request.GetResponseAsync();
        }

        public void HidePage(string page, string revision)
        {
            var request = WebAgent.CreatePost(string.Format(HideWikiPageUrl, Subreddit.Name));
            WebAgent.WritePostBody(request.GetRequestStream(), new
            {
                page = page,
                revision = revision,
                uh = Reddit.User.Modhash
            });
            var response = request.GetResponse();
        }

        public async Task HidePageAsync(string page, string revision)
        {
            var request = await WebAgent.CreatePostAsync(string.Format(HideWikiPageUrl, Subreddit.Name));
            var stream = await request.GetRequestStreamAsync();
            await WebAgent.WritePostBodyAsync(stream, new
            {
                page = page,
                revision = revision,
                uh = Reddit.User.Modhash
            });
            var response = request.GetResponseAsync();
        }

        public void RevertPage(string page, string revision)
        {
            var request = WebAgent.CreatePost(string.Format(RevertWikiPageUrl, Subreddit.Name));
            WebAgent.WritePostBody(request.GetRequestStream(), new
            {
                page = page,
                revision = revision,
                uh = Reddit.User.Modhash
            });
            var response = request.GetResponse();
        }

        public async Task RevertPageAsync(string page, string revision)
        {
            var request = await WebAgent.CreatePostAsync(string.Format(RevertWikiPageUrl, Subreddit.Name));
            var stream = await request.GetRequestStreamAsync();
            await WebAgent.WritePostBodyAsync(stream, new
            {
                page = page,
                revision = revision,
                uh = Reddit.User.Modhash
            });
            var response = request.GetResponseAsync();
        }

        public void SetPageEditor(string page, string username, bool allow)
        {
            var request = WebAgent.CreatePost(string.Format(allow ? WikiPageAllowEditorAddUrl : WikiPageAllowEditorDelUrl, Subreddit.Name));
            WebAgent.WritePostBody(request.GetRequestStream(), new
            {
                page = page,
                username = username,
                uh = Reddit.User.Modhash
            });
            var response = request.GetResponse();
        }

        public async Task SetPageEditorAsync(string page, string username, bool allow)
        {
            var request = await WebAgent.CreatePostAsync(string.Format(allow ? WikiPageAllowEditorAddUrl : WikiPageAllowEditorDelUrl, Subreddit.Name));
            var stream = await request.GetRequestStreamAsync();
            await WebAgent.WritePostBodyAsync(stream, new
            {
                page = page,
                username = username,
                uh = Reddit.User.Modhash
            });
            var response = request.GetResponse();
        }
        #region Obsolete Getter Methods

        [Obsolete("Use PageNames property instead")]
        public IEnumerable<string> GetPageNames()
        {
            return PageNames;
        }

        [Obsolete("Use Revisions property instead")]
        public Listing<WikiPageRevision> GetRevisions()
        {
            return Revisions;
        }

        #endregion Obsolete Getter Methods
    }
}