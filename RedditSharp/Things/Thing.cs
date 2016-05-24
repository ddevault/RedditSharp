using System;
using Newtonsoft.Json.Linq;
using System.Threading.Tasks;

namespace RedditSharp.Things
{
    public class Thing
    {
        public static Thing Parse(Reddit reddit, JToken json, IWebAgent webAgent)
        {
            var kind = json["kind"].ValueOrDefault<string>();
            switch (kind)
            {
                case "t1":
                    return new Comment().Init(reddit, json, webAgent, null);
                case "t2":
                    return new RedditUser().Init(reddit, json, webAgent);
                case "t3":
                    return new Post().Init(reddit, json, webAgent);
                case "t4":
                    return new PrivateMessage().Init(reddit, json, webAgent);
                case "t5":
                    return new Subreddit().Init(reddit, json, webAgent);
                case "modaction":
                    return new ModAction().Init(reddit, json, webAgent);
                default:
                    return null;
            }
        }

        // if we can't determine the type of thing by "kind", try by type
        public static Thing Parse<T>(Reddit reddit, JToken json, IWebAgent webAgent) where T : Thing
        {
            Thing result = Parse(reddit, json, webAgent);
            if (result == null)
            {
                if (typeof(T) == typeof(WikiPageRevision))
                {
                    return new WikiPageRevision().Init(reddit, json, webAgent);
                }
                else if (typeof(T) == typeof(ModAction))
                {
                    return new ModAction().Init(reddit, json, webAgent);
                }
                else if (typeof(T) == typeof(Contributor)) 
                {
                    return new Contributor().Init(reddit, json, webAgent);
                }
            }
            return result;
        }

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

        public static async Task<Thing> ParseAsync(Reddit reddit, JToken json, IWebAgent webAgent)
        {
            var kind = json["kind"].ValueOrDefault<string>();
            switch (kind)
            {
                case "t1":
                    return await new Comment().InitAsync(reddit, json, webAgent, null);
                case "t2":
                    return await new RedditUser().InitAsync(reddit, json, webAgent);
                case "t3":
                    return await new Post().InitAsync(reddit, json, webAgent);
                case "t4":
                    return await new PrivateMessage().InitAsync(reddit, json, webAgent);
                case "t5":
                    return await new Subreddit().InitAsync(reddit, json, webAgent);
                case "modaction":
                    return await new ModAction().InitAsync(reddit, json, webAgent);
                default:
                    return null;
            }
        }

        // if we can't determine the type of thing by "kind", try by type
        public static async Task<Thing> ParseAsync<T>(Reddit reddit, JToken json, IWebAgent webAgent) where T : Thing
        {
            Thing result = await ParseAsync(reddit, json, webAgent);
            if (result == null)
            {
                if (typeof(T) == typeof(WikiPageRevision))
                {
                    return await new WikiPageRevision().InitAsync(reddit, json, webAgent);
                }
                else if (typeof(T) == typeof(ModAction))
                {
                    return await new ModAction().InitAsync(reddit, json, webAgent);
                }
            }
            return result;
        }
    }
}
