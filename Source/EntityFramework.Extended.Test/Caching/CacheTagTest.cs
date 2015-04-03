using System.Globalization;
using EntityFramework.Caching;
using FluentAssertions;
using Xunit;
using System;

namespace EntityFramework.Test.Caching
{
    
    public class CacheTagTest
    {
        [Fact]
        public void CacheKeyConstructorNullTagTest()
        {
            Action action = () => new CacheTag(null);
            action.ShouldThrow<ArgumentNullException>();
        }

        [Fact]
        public void CacheKeyConstructorEmptyTagTest()
        {
            var cacheTag = new CacheTag(string.Empty);
            cacheTag.Should().NotBeNull();
            cacheTag.ToString().Should().BeEmpty();
        }


        [Fact]
        public void CacheTagConstructorTest()
        {
            string tag = DateTime.Now.Ticks.ToString(CultureInfo.InvariantCulture); 
            var cacheTag = new CacheTag(tag);
            cacheTag.Should().NotBeNull();
            cacheTag.ToString().Should().Be(tag);
        }


        [Fact]
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


        [Fact]
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
