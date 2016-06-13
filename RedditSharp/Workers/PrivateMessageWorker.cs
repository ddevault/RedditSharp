using Newtonsoft.Json.Linq;
using RedditSharp.Models;
using System;
using System.Linq;
using System.Net;
using System.Threading.Tasks;

namespace RedditSharp.Workers
{
    public class PrivateMessageWorker
    {
        private const string ComposeMessageUrl = "/api/compose";

        private Reddit reddit;

        public PrivateMessageWorker(Reddit reddit)
        {
            this.reddit = reddit;
        }

        public void ComposePrivateMessage(string subject, string body, string to, string captchaId = "", string captchaAnswer = "")
        {
            if (reddit.User == null)
                throw new Exception("User can not be null.");
            var request = reddit.WebAgent.CreatePost(ComposeMessageUrl);
            reddit.WebAgent.WritePostBody(request.GetRequestStream(), new
            {
                api_type = "json",
                subject,
                text = body,
                to,
                uh = reddit.User.Modhash,
                iden = captchaId,
                captcha = captchaAnswer
            });
            var response = request.GetResponse();
            var result = reddit.WebAgent.GetResponseString(response.GetResponseStream());
            var json = JObject.Parse(result);

            ICaptchaSolver solver = reddit.CaptchaSolver; // Prevent race condition

            if (json["json"]["errors"].Any() && json["json"]["errors"][0][0].ToString() == "BAD_CAPTCHA" && solver != null)
            {
                captchaId = json["json"]["captcha"].ToString();
                CaptchaResponse captchaResponse = solver.HandleCaptcha(new Captcha(captchaId));

                if (!captchaResponse.Cancel) // Keep trying until we are told to cancel
                    ComposePrivateMessage(subject, body, to, captchaId, captchaResponse.Answer);
            }
        }

    }
}
