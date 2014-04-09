﻿using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;

namespace RedditSharp
{
    public class Listing<T> : IEnumerable<T> where T : Thing
    {
        private IWebAgent WebAgent { get; set; }
        private Reddit Reddit { get; set; }
        private string Url { get; set; }

        internal Listing(Reddit reddit, string url, IWebAgent webAgent)
        {
            WebAgent = webAgent;
            Reddit = reddit;
            Url = url;
        }

        public IEnumerator<T> GetEnumerator()
        {
            return new ListingEnumerator<T>(this);
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

            public ListingEnumerator(Listing<T> listing)
            {
                Listing = listing;
                CurrentPageIndex = -1;
                CurrentPage = new Thing[0];
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
