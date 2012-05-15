using System;
using System.Globalization;
using EntityFramework.Caching;
using FluentAssertions;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace EntityFramework.Test.Caching
{


    [TestClass]
    public class CacheKeyTest
    {
        public TestContext TestContext { get; set; }
        
        [TestMethod]
        public void CacheKeyConstructorNullKeyTest()
        {
            Action action = () => new CacheKey(null);
            action.ShouldThrow<ArgumentNullException>();
        }

        [TestMethod]
        public void CacheKeyConstructorNullTagsTest()
        {
            Action action = () => new CacheKey("test", null);
            action.ShouldThrow<ArgumentNullException>();
        }

        [TestMethod]
        public void CacheKeyConstructorTest1()
        {
            string key = string.Empty;
            var target = new CacheKey(key);
            target.Should().NotBeNull();
            target.Key.Should().NotBeNull();
            target.Key.Should().Be(string.Empty);
        }

        [TestMethod]
        public void KeyTest()
        {
            string key = DateTime.Now.Ticks.ToString(CultureInfo.InvariantCulture);
            var target = new CacheKey(key);
            target.Should().NotBeNull();
            target.Key.Should().NotBeNull();
            target.Key.Should().Be(key);
        }

        [TestMethod]
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
