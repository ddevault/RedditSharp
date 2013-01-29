using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Newtonsoft.Json.Linq;
using System.Net;

namespace RedditSharp
{
    public class SubredditStyle
    {
        private const string UploadImageUrl = "http://www.reddit.com/api/upload_sr_img";

        private Reddit Reddit { get; set; }

        public SubredditStyle(Reddit reddit, Subreddit subreddit, JToken json)
        {
            Reddit = reddit;
            Subreddit = subreddit;
            Images = new List<SubredditImage>();
            var data = json["data"];
            CSS = data["stylesheet"].Value<string>();
            foreach (var image in data["images"])
            {
                Images.Add(new SubredditImage(
                    Reddit, this, image["link"].Value<string>(),
                    image["name"].Value<string>(), image["url"].Value<string>()));
            }
        }

        public string CSS { get; set; }
        public List<SubredditImage> Images { get; set; }
        public Subreddit Subreddit { get; set; }

        public void UploadImage(string name, ImageType imageType, byte[] file)
        {
            var request = Reddit.CreatePost(UploadImageUrl);
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
            try
            {
                var response = request.GetResponse();
                var data = Reddit.GetResponseString(response.GetResponseStream());
            }
            catch (WebException ex)
            {
                var data = Reddit.GetResponseString(ex.Response.GetResponseStream());
                throw;
            }
        }

        public enum ImageType
        {
            PNG,
            JPEG
        }
    }
}
