using System.Globalization;
using EntityFramework.Caching;
using FluentAssertions;
using NUnit.Framework;
using System;

namespace EntityFramework.Test.Caching
{
    [TestFixture]
    public class CacheTagTest
    {
        public TestContext TestContext { get; set; }

        [Test]
        public void CacheKeyConstructorNullTagTest()
        {
            Action action = () => new CacheTag(null);
            action.ShouldThrow<ArgumentNullException>();
        }

        [Test]
        public void CacheKeyConstructorEmptyTagTest()
        {
            var cacheTag = new CacheTag(string.Empty);
            cacheTag.Should().NotBeNull();
            cacheTag.ToString().Should().BeEmpty();
        }


        [Test]
        public void CacheTagConstructorTest()
        {
            string tag = DateTime.Now.Ticks.ToString(CultureInfo.InvariantCulture); 
            var cacheTag = new CacheTag(tag);
            cacheTag.Should().NotBeNull();
            cacheTag.ToString().Should().Be(tag);
        }


        [Test]
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


        [Test]
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
