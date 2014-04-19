using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;

namespace RedditSharp
{
    public class Listing<T> : IEnumerable<T> where T : Thing
    {
        private IAsyncWebAgent WebAgent { get; set; }
        private Reddit Reddit { get; set; }
        private string Url { get; set; }

        internal Listing(Reddit reddit, string url, IAsyncWebAgent webAgent)
        {
            WebAgent = webAgent;
            Reddit = reddit;
            Url = url;
        }

        public IEnumerator<T> GetEnumerator()
        {
            return new ListingEnumerator<T>(this);
        }

        public IEnumerator<T> GetEnumerator(int limit)
        {
            return new ListingEnumerator<T>(this, limit);
        }

        System.Collections.IEnumerator System.Collections.IEnumerable.GetEnumerator()
        {
            return GetEnumerator();
        }

#pragma warning disable 0693
        private class ListingEnumerator<T> : IEnumerator<T> where T : Thing
        {
            private Listing<T> Listing { get; set; }
            private int CurrentPageIndex { get; set; }
            private string After { get; set; }
            private string Before { get; set; }
            private Thing[] CurrentPage { get; set; }
            private int Limit { get; set; }
            private int Count { get; set; }


            public ListingEnumerator(Listing<T> listing)
            {
                Listing = listing;
                CurrentPageIndex = -1;
                CurrentPage = new Thing[0];
            }

            public ListingEnumerator(Listing<T> listing, int limit)
            {
                if (limit <= 0) limit = 25;
                Listing = listing;
                CurrentPageIndex = -1;
                CurrentPage = new Thing[0];
                Limit = limit;
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
                    if (url.Contains("?"))
                        url += "&after=" + After;
                    else
                        url += "?after=" + After;
                }
                
                if (Limit != 25)
                {
                    if (url.Contains("?"))
                        url += "&limit=" + Limit.ToString();
                    else
                        url += "?limit=" + Limit.ToString();
                }
                
                if (Count > 0)
                {
                    if (url.Contains("?"))
                        url += "&count=" + Count.ToString();
                    else
                        url += "?count=" + Count.ToString();
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
                After = json["data"]["after"].Value<string>();
                Before = json["data"]["before"].Value<string>();
                Count += Limit;
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
                        return false;
                    FetchNextPage();
                    CurrentPageIndex = 0;
                    if (CurrentPage.Length == 0)
                        return false;
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
