using System.Collections.Generic;
using Newtonsoft.Json.Linq;
using System.Web;

namespace RedditSharp
{
    public class SubredditStyle
    {
        private const string UploadImageUrl = "/api/upload_sr_img";
        private const string UpdateCssUrl = "/api/subreddit_stylesheet";

        private Reddit Reddit { get; set; }
        private IWebAgent WebAgent { get; set; }

        public SubredditStyle(Reddit reddit, Subreddit subreddit, IWebAgent webAgent)
        {
            Reddit = reddit;
            Subreddit = subreddit;
            WebAgent = webAgent;
        }

        public SubredditStyle(Reddit reddit, Subreddit subreddit, JToken json, IWebAgent webAgent) : this(reddit, subreddit, webAgent)
        {
            Images = new List<SubredditImage>();
            var data = json["data"];
            CSS = HttpUtility.HtmlDecode(data["stylesheet"].Value<string>());
            foreach (var image in data["images"])
            {
                Images.Add(new SubredditImage(
                    Reddit, this, image["link"].Value<string>(),
                    image["name"].Value<string>(), image["url"].Value<string>(), WebAgent));
            }
        }

        public string CSS { get; set; }
        public List<SubredditImage> Images { get; set; }
        public Subreddit Subreddit { get; set; }

        public void UpdateCss()
        {
            var request = WebAgent.CreatePost(UpdateCssUrl);
            var stream = request.GetRequestStream();
            WebAgent.WritePostBody(stream, new
            {
                op = "save",
                stylesheet_contents = CSS,
                uh = Reddit.User.Modhash,
                api_type = "json",
                r = Subreddit.Name
            });
            stream.Close();
            var response = request.GetResponse();
            var data = WebAgent.GetResponseString(response.GetResponseStream());
            var json = JToken.Parse(data);
        }

        public void UploadImage(string name, ImageType imageType, byte[] file)
        {
            var request = WebAgent.CreatePost(UploadImageUrl);
            var formData = new MultipartFormBuilder(request);
            formData.AddDynamic(new
                {
                    name,
                    uh = Reddit.User.Modhash,
                    r = Subreddit.Name,
                    formid = "image-upload",
                    img_type = imageType == ImageType.PNG ? "png" : "jpg",
                    upload = ""
                });
            formData.AddFile("file", "foo.png", file, imageType == ImageType.PNG ? "image/png" : "image/jpeg");
            formData.Finish();
            var response = request.GetResponse();
            var data = WebAgent.GetResponseString(response.GetResponseStream());
            // TODO: Detect errors
        }
    }

    public enum ImageType
    {
        PNG,
        JPEG
    }
}
