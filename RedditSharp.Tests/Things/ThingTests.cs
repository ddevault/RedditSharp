using System;
using FakeItEasy;
using FakeItEasy.Auto;
using FluentAssertions;
using Newtonsoft.Json.Linq;
using RedditSharp.Things;
using NUnit.Framework;

namespace RedditSharp.Tests.Things
{
    [TestFixture]
    public class ThingTests
    {
        private const string RedditUserType = "t2";
        private const string CommentType = "t1";
        private const string PostType = "t3";
        private const string PrivateMessageType = "t4";
        private const string SubredditType = "t5";

        [Test]
        public void Parse_returns_Comment()
        {
            // Given
            var token = JToken.FromObject(new TestKind { kind = CommentType });

            // When
            var thing = Thing.Parse(An.AutoFaked<Reddit>(), token, A.Fake<IWebAgent>());

            // Then
            thing.Should().BeOfType<Comment>();
        }

        [Test]
        public void Parse_returns_RedditUser()
        {
            // Given
            var token = JToken.FromObject(new TestKind { kind = RedditUserType });

            // When
            var thing = Thing.Parse(An.AutoFaked<Reddit>(), token, A.Fake<IWebAgent>());

            // Then
            thing.Should().BeOfType<RedditUser>();
        }

        [Test]
        public void Parse_returns_Post()
        {
            // Given
            var token = JToken.FromObject(new TestKind { kind = PostType });

            // When
            var thing = Thing.Parse(An.AutoFaked<Reddit>(), token, A.Fake<IWebAgent>());

            // Then
            thing.Should().BeOfType<Post>();
        }

        [Test]
        public void Parse_returns_PrivateMessage()
        {
            // Given
            var token = JToken.FromObject(new TestKind { kind = PrivateMessageType });

            // When
            var thing = Thing.Parse(An.AutoFaked<Reddit>(), token, A.Fake<IWebAgent>());

            // Then
            thing.Should().BeOfType<PrivateMessage>();
        }

        [Test]
        // This test will currently fail to serialize correctly.
        public void Parse_returns_Subreddit()
        {
            // Given
            var testKind = new TestKind
            {
                kind = SubredditType,
                data = new FakeSubreddit { Url = new Uri("http://www.reddit.com/r/pics") }
            };

            var token = JToken.FromObject(testKind);

            // When
            var thing = Thing.Parse(An.AutoFaked<Reddit>(), token, A.Fake<IWebAgent>());

            // Then
            thing.Should().BeOfType<Subreddit>();
        }

        [Test]
        public void Parse_default_returns_null()
        {
            // Given
            var token = JToken.FromObject(new TestKind { kind = "something_else" });

            // When
            var thing = Thing.Parse(An.AutoFaked<Reddit>(), token, A.Fake<IWebAgent>());

            // Then
            thing.Should().BeNull();
        }

        #region Fake classes for testing.

        // ReSharper disable MemberCanBePrivate.Local
        // ReSharper disable UnusedAutoPropertyAccessor.Local
        // ReSharper disable InconsistentNaming

        private class FakeSubreddit
        {
            public Uri Url { get; set; }
        }

        private class TestKind
        {
            public TestKind()
            {
                kind = string.Empty;
                name = string.Empty;
                data = new object();
            }

            public string kind { get; set; }
            public string name { get; set; }
            public object data { get; set; }
        }

        // ReSharper restore InconsistentNaming
        // ReSharper restore UnusedAutoPropertyAccessor.Local
        // ReSharper restore MemberCanBePrivate.Local

        #endregion
    }
}