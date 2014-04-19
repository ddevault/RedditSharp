using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;

using RedditSharp;

namespace RedditSharp.UnitTests
{
    [TestClass]
    public class RedditTests
    {
        [TestMethod]
        public void TestFrontPage()
        {
            var reddit = new Reddit();
            var frontPage = reddit.FrontPage;
            Assert.IsNotNull(frontPage);
            Assert.IsInstanceOfType(frontPage, typeof(Subreddit));
        }

        [TestMethod]
        public void TestRSlashAll()
        {
            var reddit = new Reddit();
            var rAll = reddit.RSlashAll;
            Assert.IsNotNull(rAll);
            Assert.IsInstanceOfType(rAll, typeof(Subreddit));
        }

        [TestMethod]
        public void TestGetUser()
        {
            var reddit = new Reddit();
            var tUser = reddit.GetUser("hueypriest");
            Assert.IsNotNull(tUser);
            Assert.IsInstanceOfType(tUser, typeof(RedditUser));
            Assert.IsFalse(string.IsNullOrEmpty(tUser.FullName));
        }

        [TestMethod] 
        public void TestGetUserAsync()
        {
            var reddit = new Reddit();
            var tUser = reddit.GetUserAsync("hueypriest").Result;
            Assert.IsNotNull(tUser);
            Assert.IsInstanceOfType(tUser, typeof(RedditUser));
            Assert.IsFalse(string.IsNullOrEmpty(tUser.FullName));
        }

        [TestMethod]
        public void TestGetSubreddit()
        {
            var reddit = new Reddit();
            var tSub = reddit.GetSubreddit("pics");
            Assert.IsNotNull(tSub);
            Assert.IsInstanceOfType(tSub, typeof(Subreddit));
            Assert.IsFalse(string.IsNullOrEmpty(tSub.FullName));
        }

        [TestMethod]
        public void TestGetSubredditAsync()
        {
            var reddit = new Reddit();
            var tSub = reddit.GetSubredditAsync("pics").Result;
            Assert.IsNotNull(tSub);
            Assert.IsInstanceOfType(tSub, typeof(Subreddit));
            Assert.IsFalse(string.IsNullOrEmpty(tSub.FullName));
        }
    }
}
