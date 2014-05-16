using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;

namespace RedditSharp
{
    public class Listing<T> : IEnumerable<T> where T : Thing
    {
        /// <summary>
        /// Gets the default number of listings returned per request
        /// </summary>
        internal const int DefaultListingPerRequest = 25;

        private IWebAgent WebAgent { get; set; }
        private Reddit Reddit { get; set; }
        private string Url { get; set; }

        /// <summary>
        /// Creates a new Listing instance
        /// </summary>
        /// <param name="reddit"></param>
        /// <param name="url"></param>
        /// <param name="webAgent"></param>
        internal Listing(Reddit reddit, string url, IWebAgent webAgent)
        {
            WebAgent = webAgent;
            Reddit = reddit;
            Url = url;
        }

        /// <summary>
        /// Returns an enumerator that iterates through a collection, using the specified number of listings per
        /// request and optionally the maximum number of listings
        /// </summary>
        /// <param name="limitPerRequest">The number of listings to be returned per request</param>
        /// <param name="maximumLimit">The maximum number of listings to return</param>
        /// <returns></returns>
        public IEnumerator<T> GetEnumerator(int limitPerRequest, int maximumLimit = -1)
        {
            return new ListingEnumerator<T>(this, limitPerRequest, maximumLimit);
        }

        /// <summary>
        /// Returns an enumerator that iterates through a collection, using the default number of listings per request
        /// </summary>
        /// <returns></returns>
        public IEnumerator<T> GetEnumerator()
        {
            return GetEnumerator(DefaultListingPerRequest);
        }

        /// <summary>
        /// Returns an enumerator that iterates through a collection
        /// </summary>
        /// <returns></returns>
        System.Collections.IEnumerator System.Collections.IEnumerable.GetEnumerator()
        {
            return GetEnumerator();
        }

        /// <summary>
        /// Returns an IEnumerable instance which will return the specified maximum number of listings
        /// </summary>
        /// <param name="maximumLimit"></param>
        /// <returns></returns>
        public IEnumerable<T> GetListing(int maximumLimit)
        {
            return GetListing(maximumLimit, DefaultListingPerRequest);
        }

        /// <summary>
        /// Returns an IEnumerable instance which will return the specified maximum number of listings
        /// with the limited number per request
        /// </summary>
        /// <param name="maximumLimit"></param>
        /// <param name="limitPerRequest"></param>
        /// <returns></returns>
        public IEnumerable<T> GetListing(int maximumLimit, int limitPerRequest)
        {
            // Get the enumerator with the specified maximum and per request limits
            var enumerator = GetEnumerator(limitPerRequest, maximumLimit);

            return GetEnumerator(enumerator);
        }

        /// <summary>
        /// Converts an IEnumerator instance to an IEnumerable
        /// </summary>
        /// <param name="enumerator"></param>
        /// <returns></returns>
        private static IEnumerable<T> GetEnumerator(IEnumerator<T> enumerator)
        {
            while (enumerator.MoveNext())
            {
                yield return enumerator.Current;
            }
        }

#pragma warning disable 0693
        private class ListingEnumerator<T> : IEnumerator<T> where T : Thing
        {
            private Listing<T> Listing { get; set; }
            private int CurrentPageIndex { get; set; }
            private string After { get; set; }
            private string Before { get; set; }
            private Thing[] CurrentPage { get; set; }
            private int Count { get; set; }
            private int LimitPerRequest { get; set; }
            private int MaximumLimit { get; set; }

            /// <summary>
            /// Creates a new ListingEnumerator instance
            /// </summary>
            /// <param name="listing"></param>
            /// <param name="limitPerRequest">The number of listings to be returned per request. -1 will exclude this parameter and use the Reddit default (25)</param>
            /// <param name="maximumLimit">The maximum number of listings to return, -1 will not add a limit</param>
            public ListingEnumerator(Listing<T> listing, int limitPerRequest, int maximumLimit)
            {
                Listing = listing;
                CurrentPageIndex = -1;
                CurrentPage = new Thing[0];

                // Set the listings per page (if not specified, use the Reddit default of 25) and the maximum listings
                LimitPerRequest = (limitPerRequest <= 0 ? DefaultListingPerRequest : limitPerRequest); 
                MaximumLimit = maximumLimit;
            }

            public T Current
            {
                get 
                {
                    return (T)CurrentPage[CurrentPageIndex];
                }
            }

            private void FetchNextPage()
            {
                var url = Listing.Url;

                if (After != null)
                {
                    url += (url.Contains("?") ? "&" : "?") + "after=" + After;
                }

                if (LimitPerRequest != -1)
                {
                    int limit = LimitPerRequest;

                    if (limit > MaximumLimit)
                    {
                        // If the limit is more than the maximum number of listings, adjust
                        limit = MaximumLimit;
                    }
                    else if (Count + limit > MaximumLimit)
                    {
                        // If a smaller subset of listings are needed, adjust the limit
                        limit = MaximumLimit - Count;
                    }

                    if (limit > 0)
                    {
                        // Add the limit, the maximum number of items to be returned per page
                        url += (url.Contains("?") ? "&" : "?") + "limit=" + limit;
                    }
                }

                if (Count > 0)
                {
                    // Add the count, the number of items already seen in this listing
                    // The Reddit API uses this to determine when to give values for before and after fields                
                    url += (url.Contains("?") ? "&" : "?") + "count=" + Count;
                }

                var request = Listing.WebAgent.CreateGet(url);
                var response = request.GetResponse();
                var data = Listing.WebAgent.GetResponseString(response.GetResponseStream());
                var json = JToken.Parse(data);
                if (json["kind"].ValueOrDefault<string>() != "Listing")
                    throw new FormatException("Reddit responded with an object that is not a listing.");
                Parse(json);
            }

            private void Parse(JToken json)
            {
                var children = json["data"]["children"] as JArray;
                CurrentPage = new Thing[children.Count];
                
                for (int i = 0; i < CurrentPage.Length; i++)
                    CurrentPage[i] = Thing.Parse<T>(Listing.Reddit, children[i], Listing.WebAgent);

                // Increase the total count of items returned
                Count += CurrentPage.Length;

                After = json["data"]["after"].Value<string>();
                Before = json["data"]["before"].Value<string>();
            }

            public void Dispose()
            {
                // ...
            }

            object System.Collections.IEnumerator.Current
            {
                get { return Current; }
            }

            public bool MoveNext()
            {
                CurrentPageIndex++;
                if (CurrentPageIndex == CurrentPage.Length)
                {
                    if (After == null && CurrentPageIndex != 0)
                    {
                        // No more pages to return
                        return false;
                    }

                    if (MaximumLimit != -1 && Count >= MaximumLimit)
                    {
                        // Maximum listing count returned
                        return false;
                    }

                    // Get the next page
                    FetchNextPage();
                    CurrentPageIndex = 0;

                    if (CurrentPage.Length == 0)
                    {
                        // No listings were returned in the page
                        return false;
                    }
                }
                return true;
            }

            public void Reset()
            {
                After = Before = null;
                CurrentPageIndex = -1;
                CurrentPage = new Thing[0];
            }
        }
#pragma warning restore
    }
}
