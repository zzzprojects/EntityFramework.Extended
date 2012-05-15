using System.Globalization;
using EntityFramework.Caching;
using FluentAssertions;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;

namespace EntityFramework.Test.Caching
{
    [TestClass]
    public class CacheTagTest
    {
        public TestContext TestContext { get; set; }

        [TestMethod]
        public void CacheKeyConstructorNullTagTest()
        {
            Action action = () => new CacheTag(null);
            action.ShouldThrow<ArgumentNullException>();
        }

        [TestMethod]
        public void CacheKeyConstructorEmptyTagTest()
        {
            var cacheTag = new CacheTag(string.Empty);
            cacheTag.Should().NotBeNull();
            cacheTag.ToString().Should().BeEmpty();
        }


        [TestMethod]
        public void CacheTagConstructorTest()
        {
            string tag = DateTime.Now.Ticks.ToString(CultureInfo.InvariantCulture); 
            var cacheTag = new CacheTag(tag);
            cacheTag.Should().NotBeNull();
            cacheTag.ToString().Should().Be(tag);
        }


        [TestMethod]
        public void EqualsTest()
        {
            string tag = DateTime.Now.Ticks.ToString(CultureInfo.InvariantCulture);
            var leftTag = new CacheTag(tag);
            leftTag.Should().NotBeNull();
            leftTag.ToString().Should().Be(tag);

            var rightTag = new CacheTag(tag);
            rightTag.Should().NotBeNull();
            rightTag.ToString().Should().Be(tag);

            leftTag.Equals(rightTag).Should().BeTrue();

            (leftTag == rightTag).Should().BeTrue();
        }


        [TestMethod]
        public void GetHashCodeTest()
        {
            string tag = DateTime.Now.Ticks.ToString(CultureInfo.InvariantCulture);
            int hashCode = tag.GetHashCode();

            var cacheTag = new CacheTag(tag);
            cacheTag.Should().NotBeNull();
            cacheTag.ToString().Should().Be(tag);
            cacheTag.GetHashCode().Should().Be(hashCode);
        }
    }
}
