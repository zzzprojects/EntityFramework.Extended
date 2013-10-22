using System.Runtime.Caching;
using EntityFramework.Caching;
using FluentAssertions;
using NUnit.Framework;
using System;

namespace EntityFramework.Test
{


    [TestFixture]
    public class CacheManagerTest
    {
        public TestContext TestContext { get; set; }

        [Test]
        public void ConstructorTest()
        {
            var cacheManager = new CacheManager();
            cacheManager.Should().NotBeNull();
        }

        [Test]
        public void AddTest()
        {
            var cacheManager = new CacheManager();
            var cacheKey = new CacheKey("AddTest" + DateTime.Now.Ticks);
            var value = "Test Value " + DateTime.Now;
            var cachePolicy = new CachePolicy();

            bool result = cacheManager.Add(cacheKey, value, cachePolicy);
            result.Should().BeTrue();
        }

        [Test]
        public void ExpireTest()
        {
            var cacheManager = new CacheManager();
            string key = "AddTest" + DateTime.Now.Ticks;
            var tags = new[] { "a", "b" };
            var cacheKey = new CacheKey(key, tags);
            var value = "Test Value " + DateTime.Now;
            var cachePolicy = new CachePolicy();

            bool result = cacheManager.Add(cacheKey, value, cachePolicy);
            result.Should().BeTrue();

            // add second value with same tag
            string key2 = "AddTest2" + DateTime.Now.Ticks;
            var tags2 = new[] { "a", "c" };
            var cacheKey2 = new CacheKey(key2, tags2);
            var value2 = "Test Value 2 " + DateTime.Now;
            var cachePolicy2 = new CachePolicy();

            bool result2 = cacheManager.Add(cacheKey2, value2, cachePolicy2);
            result2.Should().BeTrue();

            // add third value with same tag
            string key3 = "AddTest3" + DateTime.Now.Ticks;
            var tags3 = new[] { "b", "c" };
            var cacheKey3 = new CacheKey(key3, tags3);
            var value3 = "Test Value 3 " + DateTime.Now;
            var cachePolicy3 = new CachePolicy();

            bool result3 = cacheManager.Add(cacheKey3, value3, cachePolicy3);
            result3.Should().BeTrue();


            var cacheTag = new CacheTag("a");
            string tagKey = MemoryCacheProvider.GetTagKey(cacheTag);
            tagKey.Should().NotBeNullOrEmpty();

            var cachedTag = cacheManager.Get(tagKey);
            cachedTag.Should().NotBeNull();

            // expire actually just changes the value for tag key
            cacheManager.Expire(cacheTag);

            var expiredTag = cacheManager.Get(tagKey);
            expiredTag.Should().NotBeNull();
            expiredTag.Should().NotBe(cachedTag);

            // items should have been removed
            var expiredValue = cacheManager.Get(cacheKey.Key);
            expiredValue.Should().BeNull();

            var expiredValue2 = cacheManager.Get(cacheKey2.Key);
            expiredValue2.Should().BeNull();

            var expiredValue3 = cacheManager.Get(cacheKey3.Key);
            expiredValue3.Should().NotBeNull();
        }

        [Test]
        public void GetTest()
        {
            var cacheManager = new CacheManager();
            var cacheKey = new CacheKey("GetTest" + DateTime.Now.Ticks);
            var value = "Get Value " + DateTime.Now;
            var cachePolicy = new CachePolicy();

            bool result = cacheManager.Add(cacheKey, value, cachePolicy);
            result.Should().BeTrue();

            var existing = cacheManager.Get(cacheKey.Key);
            existing.Should().NotBeNull();
            existing.Should().BeSameAs(value);

        }

        [Test]
        public void GetOrAddTest()
        {
            var cacheManager = new CacheManager();
            var cacheKey = new CacheKey("AddTest" + DateTime.Now.Ticks);
            var value = "Test Value " + DateTime.Now;
            var cachePolicy = new CachePolicy();
            int callCount = 0;

            Func<CacheKey, object> valueFactory = k =>
            {
                callCount++;
                return value;
            };

            var result = cacheManager.GetOrAdd(cacheKey, valueFactory, cachePolicy);
            result.Should().Be(value);
            callCount.Should().Be(1);

            var cachedValue = cacheManager.Get(cacheKey.Key);
            cachedValue.Should().NotBeNull();
            cachedValue.Should().Be(value);

            callCount = 0;
            var result2 = cacheManager.GetOrAdd(cacheKey, valueFactory, cachePolicy);
            result2.Should().Be(value);
            callCount.Should().Be(0);

        }

        [Test]
        public void RemoveTest()
        {
            var cacheManager = new CacheManager();
            var cacheKey = new CacheKey("AddTest" + DateTime.Now.Ticks);
            var value = "Test Value " + DateTime.Now;
            var cachePolicy = new CachePolicy();

            bool result = cacheManager.Add(cacheKey, value, cachePolicy);
            result.Should().BeTrue();

            // look in underlying MemoryCache
            var cachedValue = cacheManager.Get(cacheKey.Key);
            cachedValue.Should().NotBeNull();
            cachedValue.Should().Be(value);

            var removed = cacheManager.Remove(cacheKey);
            removed.Should().NotBeNull();
            removed.Should().Be(value);

            // look in underlying MemoryCache
            var previous = cacheManager.Get(cacheKey.Key);
            previous.Should().BeNull();
        }


        [Test]
        public void SetTest()
        {
            var cacheManager = new CacheManager();
            var cacheKey = new CacheKey("SetTest" + DateTime.Now.Ticks);
            var value = "Set Value " + DateTime.Now;
            var cachePolicy = new CachePolicy();

            cacheManager.Set(cacheKey, value, cachePolicy);
            
            var cachedValue = cacheManager.Get(cacheKey.Key);
            cachedValue.Should().NotBeNull();
            cachedValue.Should().Be(value);

            var value2 = "Set Value " + DateTime.Now;
            cacheManager.Set(cacheKey, value2, cachePolicy);

            var cachedValue2 = cacheManager.Get(cacheKey.Key);
            cachedValue2.Should().NotBeNull();
            cachedValue2.Should().Be(value2);
        }
    }
}
