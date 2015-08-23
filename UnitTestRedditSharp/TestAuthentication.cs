using Microsoft.VisualStudio.TestTools.UnitTesting;
using RedditSharp;

namespace UnitTestRedditSharp
{
    [TestClass]
    public class TestAuthentication
    {
        [TestMethod]
        [Timeout(15000)]
        [ExpectedException(typeof(System.Security.Authentication.AuthenticationException))]
        public void ShouldFailOnWrongCredentials()
        {
            Reddit reddit = new Reddit("test", "test");
        }

        [TestMethod]
        [Timeout(15000)]
        public void ShouldAuthenticateOnRightCredentials()
        {
            Reddit reddit = new Reddit(UnshareableSensitiveInfo.USER, UnshareableSensitiveInfo.PASS);
            Assert.IsNotNull(reddit.User);
            Assert.AreEqual(UnshareableSensitiveInfo.USER, reddit.User.FullName);
        }
    }
}
