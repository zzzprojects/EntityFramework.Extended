using System;
using System.Globalization;
using EntityFramework.Caching;
using FluentAssertions;
using NUnit.Framework;

namespace EntityFramework.Test.Caching
{


    [TestFixture]
    public class CacheKeyTest
    {
        public TestContext TestContext { get; set; }
        
        [Test]
        public void CacheKeyConstructorNullKeyTest()
        {
            Action action = () => new CacheKey(null);
            action.ShouldThrow<ArgumentNullException>();
        }

        [Test]
        public void CacheKeyConstructorNullTagsTest()
        {
            Action action = () => new CacheKey("test", null);
            action.ShouldThrow<ArgumentNullException>();
        }

        [Test]
        public void CacheKeyConstructorTest1()
        {
            string key = string.Empty;
            var target = new CacheKey(key);
            target.Should().NotBeNull();
            target.Key.Should().NotBeNull();
            target.Key.Should().Be(string.Empty);
        }

        [Test]
        public void KeyTest()
        {
            string key = DateTime.Now.Ticks.ToString(CultureInfo.InvariantCulture);
            var target = new CacheKey(key);
            target.Should().NotBeNull();
            target.Key.Should().NotBeNull();
            target.Key.Should().Be(key);
        }

        [Test]
        public void TagsTest()
        {
            string key = DateTime.Now.Ticks.ToString(CultureInfo.InvariantCulture);
            string[] tags = new[] { "a", "b" };
            var target = new CacheKey(key, tags);

            target.Should().NotBeNull();
            target.Key.Should().NotBeNull();
            target.Key.Should().Be(key);

            target.Tags.Should().HaveCount(2);
        }
    }
}
