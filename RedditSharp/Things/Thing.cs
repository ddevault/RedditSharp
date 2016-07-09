using System;
using Newtonsoft.Json.Linq;
using System.Threading.Tasks;

namespace RedditSharp.Things
{
    public class Thing
    {
        internal void Init(JToken json)
        {
            if (json == null)
                return;
            var data = json["name"] == null ? json["data"] : json;
            FullName = data["name"].ValueOrDefault<string>();
            Id = data["id"].ValueOrDefault<string>();
            Kind = json["kind"].ValueOrDefault<string>();
            FetchedAt = DateTime.Now;
        }
        /// <summary>
        /// Shortlink to the item
        /// </summary>
        public virtual string Shortlink
        {
            get { return "http://redd.it/" + Id; }
        }

        public string Id { get; set; }
        public string FullName { get; set; }
        public string Kind { get; set; }

        /// <summary>
        /// The time at which this object was fetched from reddit servers.
        /// </summary>
        public DateTime FetchedAt { get; private set; }

        /// <summary>
        /// Gets the time since last fetch from reddit servers.
        /// </summary>
        public TimeSpan TimeSinceFetch
        {
            get
            {
                return DateTime.Now - FetchedAt;
            }
        }
        // Awaitables don't have to be called asyncronously
        /// <summary>
        /// Parses what it is, based on the t(number) attribute
        /// </summary>
        /// <param name="reddit">Reddit you're using</param>
        /// <param name="json">Json Token</param>
        /// <param name="webAgent">WebAgent</param>
        /// <returns>A "Thing", such as a comment, user, post, etc.</returns>
        public static async Task<Thing> Parse(Reddit reddit, JToken json, IWebAgent webAgent)
        {
            var kind = json["kind"].ValueOrDefault<string>();
            switch (kind)
            {
                case "t1":
                    return await new Comment().Init(reddit, json, webAgent, null);
                case "t2":
                    return await new RedditUser().Init(reddit, json, webAgent);
                case "t3":
                    return await new Post().Init(reddit, json, webAgent);
                case "t4":
                    return await new PrivateMessage().Init(reddit, json, webAgent);
                case "t5":
                    return await new Subreddit().Init(reddit, json, webAgent);
                case "modaction":
                    return await new ModAction().Init(reddit, json, webAgent);
                default:
                    return null;
            }
        }

        /// <summary>
        /// Tries to find the "Thing" you are looking for
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="reddit"></param>
        /// <param name="json"></param>
        /// <param name="webAgent"></param>
        /// <returns>The "Thing"</returns>
        public static async Task<Thing> Parse<T>(Reddit reddit, JToken json, IWebAgent webAgent) where T : Thing
        {
            Thing result = await Parse(reddit, json, webAgent);
            if (result == null)
            {
                if (typeof(T) == typeof(WikiPageRevision))
                {
                    return await new WikiPageRevision().Init(reddit, json, webAgent);
                }
                else if (typeof(T) == typeof(ModAction))
                {
                    return await new ModAction().Init(reddit, json, webAgent);
                }
            }
            return result;
        }
    }
}
