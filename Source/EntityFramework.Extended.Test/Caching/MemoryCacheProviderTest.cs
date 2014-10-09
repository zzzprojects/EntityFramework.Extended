using System;
using System.Collections.Generic;
using System.Globalization;
using System.Runtime.Caching;
using EntityFramework.Caching;
using FluentAssertions;
using Xunit;

namespace EntityFramework.Test.Caching
{
    
    public class MemoryCacheProviderTest
    {
        [Fact]
        public void MemoryCacheProviderConstructorTest()
        {
            Action action = () => new MemoryCacheProvider();
            action.ShouldNotThrow();
        }

        [Fact]
        public void AddTest()
        {
            var provider = new MemoryCacheProvider();
            var cacheKey = new CacheKey("AddTest" + DateTime.Now.Ticks);
            var value = "Test Value " + DateTime.Now;
            var cachePolicy = new CachePolicy();

            bool result = provider.Add(cacheKey, value, cachePolicy);
            result.Should().BeTrue();

            // look in underlying MemoryCache
            string innerKey = MemoryCacheProvider.GetKey(cacheKey);
            var cachedValue = MemoryCache.Default.Get(innerKey);
            cachedValue.Should().NotBeNull();
            cachedValue.Should().Be(value);
        }

        [Fact]
        public void AddWithTagsTest()
        {
            var provider = new MemoryCacheProvider();
            string key = "AddTest" + DateTime.Now.Ticks;
            string[] tags = new[] { "a", "b" };
            var cacheKey = new CacheKey(key, tags);
            var value = "Test Value " + DateTime.Now;
            var cachePolicy = new CachePolicy();

            bool result = provider.Add(cacheKey, value, cachePolicy);
            result.Should().BeTrue();

            // look in underlying MemoryCache
            string innerKey = MemoryCacheProvider.GetKey(cacheKey);
            var cachedValue = MemoryCache.Default.Get(innerKey);
            cachedValue.Should().NotBeNull();
            cachedValue.Should().Be(value);

            // make sure cache key is in underlying MemoryCache
            var cacheTag = new CacheTag("a");
            string tagKey = MemoryCacheProvider.GetTagKey(cacheTag);
            tagKey.Should().NotBeNullOrEmpty();

            var cachedTag = MemoryCache.Default.Get(tagKey);
            cachedTag.Should().NotBeNull();
        }

        [Fact]
        public void AddWithExistingTagTest()
        {
            var provider = new MemoryCacheProvider();
            string key = "AddTest" + DateTime.Now.Ticks;
            string[] tags = new[] { "a", "b" };
            var cacheKey = new CacheKey(key, tags);
            var value = "Test Value " + DateTime.Now;
            var cachePolicy = new CachePolicy();

            bool result = provider.Add(cacheKey, value, cachePolicy);
            result.Should().BeTrue();

            // make sure cache key is in underlying MemoryCache
            var cacheTag = new CacheTag("a");
            string tagKey = MemoryCacheProvider.GetTagKey(cacheTag);
            tagKey.Should().NotBeNullOrEmpty();

            var cachedTag = MemoryCache.Default.Get(tagKey);
            cachedTag.Should().NotBeNull();

            // add second value with same tag
            string key2 = "AddTest2" + DateTime.Now.Ticks;
            string[] tags2 = new[] { "a", "c" };
            var cacheKey2 = new CacheKey(key2, tags2);
            var value2 = "Test Value 2 " + DateTime.Now;
            var cachePolicy2 = new CachePolicy();

            bool result2 = provider.Add(cacheKey2, value2, cachePolicy2);
            result2.Should().BeTrue();

            // tag 'a' should have same value
            var cachedTag2 = MemoryCache.Default.Get(tagKey);
            cachedTag2.Should().NotBeNull();
            cachedTag2.Should().Be(cachedTag);
        }

        [Fact]
        public void ExpireTest()
        {
            var cache = MemoryCache.Default;
            // purge all values
            foreach (KeyValuePair<string, object> pair in cache)
                cache.Remove(pair.Key);

            var provider = new MemoryCacheProvider();
            string key = "AddTest" + DateTime.Now.Ticks;
            var tags = new[] { "a", "b" };
            var cacheKey = new CacheKey(key, tags);
            var value = "Test Value " + DateTime.Now;
            var cachePolicy = new CachePolicy();

            bool result = provider.Add(cacheKey, value, cachePolicy);
            result.Should().BeTrue();

            // add second value with same tag
            string key2 = "AddTest2" + DateTime.Now.Ticks;
            var tags2 = new[] { "a", "c" };
            var cacheKey2 = new CacheKey(key2, tags2);
            var value2 = "Test Value 2 " + DateTime.Now;
            var cachePolicy2 = new CachePolicy();

            bool result2 = provider.Add(cacheKey2, value2, cachePolicy2);
            result2.Should().BeTrue();

            // add third value with same tag
            string key3 = "AddTest3" + DateTime.Now.Ticks;
            var tags3 = new[] { "b", "c" };
            var cacheKey3 = new CacheKey(key3, tags3);
            var value3 = "Test Value 3 " + DateTime.Now;
            var cachePolicy3 = new CachePolicy();

            bool result3 = provider.Add(cacheKey3, value3, cachePolicy3);
            result3.Should().BeTrue();


            var cacheTag = new CacheTag("a");
            string tagKey = MemoryCacheProvider.GetTagKey(cacheTag);
            tagKey.Should().NotBeNullOrEmpty();

            // underlying cache
            cache.GetCount().Should().Be(6);

            var cachedTag = cache.Get(tagKey);
            cachedTag.Should().NotBeNull();

            System.Threading.Thread.Sleep(500);

            // expire actually just changes the value for tag key
            provider.Expire(cacheTag);

            var expiredTag = cache.Get(tagKey);
            expiredTag.Should().NotBeNull();
            expiredTag.Should().NotBe(cachedTag);

            // items should have been removed
            var expiredValue = provider.Get(cacheKey);
            expiredValue.Should().BeNull();

            var expiredValue2 = provider.Get(cacheKey2);
            expiredValue2.Should().BeNull();

            var expiredValue3 = provider.Get(cacheKey3);
            expiredValue3.Should().NotBeNull();

            cache.GetCount().Should().Be(4);
        }

        [Fact]
        public void GetTest()
        {
            var provider = new MemoryCacheProvider();
            var cacheKey = new CacheKey("GetTest" + DateTime.Now.Ticks);
            var value = "Get Value " + DateTime.Now;
            var cachePolicy = new CachePolicy();

            bool result = provider.Add(cacheKey, value, cachePolicy);
            result.Should().BeTrue();

            var existing = provider.Get(cacheKey);
            existing.Should().NotBeNull();
            existing.Should().BeSameAs(value);
        }

        [Fact]
        public void GetOrAddTest()
        {
            var provider = new MemoryCacheProvider();
            var cacheKey = new CacheKey("AddTest" + DateTime.Now.Ticks);
            var value = "Test Value " + DateTime.Now;
            var cachePolicy = new CachePolicy();
            int callCount = 0;

            Func<CacheKey, object> valueFactory = k =>
            {
                callCount++;
                return value;
            };

            var result = provider.GetOrAdd(cacheKey, valueFactory, cachePolicy);
            result.Should().Be(value);
            callCount.Should().Be(1);

            // look in underlying MemoryCache
            string innerKey = MemoryCacheProvider.GetKey(cacheKey);
            var cachedValue = MemoryCache.Default.Get(innerKey);
            cachedValue.Should().NotBeNull();
            cachedValue.Should().Be(value);

            callCount = 0;
            var result2 = provider.GetOrAdd(cacheKey, valueFactory, cachePolicy);
            result2.Should().Be(value);
            callCount.Should().Be(0);
        }

        [Fact]
        public void RemoveTest()
        {
            var provider = new MemoryCacheProvider();
            var cacheKey = new CacheKey("AddTest" + DateTime.Now.Ticks);
            var value = "Test Value " + DateTime.Now;
            var cachePolicy = new CachePolicy();

            bool result = provider.Add(cacheKey, value, cachePolicy);
            result.Should().BeTrue();

            // look in underlying MemoryCache
            string innerKey = MemoryCacheProvider.GetKey(cacheKey);
            var cachedValue = MemoryCache.Default.Get(innerKey);
            cachedValue.Should().NotBeNull();
            cachedValue.Should().Be(value);

            var removed = provider.Remove(cacheKey);
            removed.Should().NotBeNull();
            removed.Should().Be(value);

            // look in underlying MemoryCache
            var previous = MemoryCache.Default.Get(innerKey);
            previous.Should().BeNull();
        }

        [Fact]
        public void SetTest()
        {
            var provider = new MemoryCacheProvider();
            var cacheKey = new CacheKey("SetTest" + DateTime.Now.Ticks);
            var value = "Set Value " + DateTime.Now;
            var cachePolicy = new CachePolicy();

            bool result = provider.Set(cacheKey, value, cachePolicy);
            result.Should().BeTrue();

            // look in underlying MemoryCache
            string innerKey = MemoryCacheProvider.GetKey(cacheKey);
            var cachedValue = MemoryCache.Default.Get(innerKey);
            cachedValue.Should().NotBeNull();
            cachedValue.Should().Be(value);

            var value2 = "Set Value " + DateTime.Now;
            bool result2 = provider.Set(cacheKey, value2, cachePolicy);
            result2.Should().BeTrue();

            var cachedValue2 = MemoryCache.Default.Get(innerKey);
            cachedValue2.Should().NotBeNull();
            cachedValue2.Should().Be(value2);
        }

        [Fact]
        public void CreateChangeMonitorTest()
        {
            string key = DateTime.Now.Ticks.ToString(CultureInfo.InvariantCulture);
            string[] tags = new[] { "a", "b" };
            var cacheKey = new CacheKey(key, tags);
            cacheKey.Should().NotBeNull();

            var monitor = MemoryCacheProvider.CreateChangeMonitor(cacheKey);
            monitor.Should().NotBeNull();
            monitor.CacheKeys.Should().HaveCount(2);

            var cacheTag = new CacheTag("a");
            string tagKey = MemoryCacheProvider.GetTagKey(cacheTag);
            tagKey.Should().NotBeNullOrEmpty();

            monitor.CacheKeys.Should().Contain(tagKey);
        }

        [Fact]
        public void CreatePolicyAbsoluteTest()
        {
            string key = DateTime.Now.Ticks.ToString(CultureInfo.InvariantCulture);
            string[] tags = new[] { "a", "b" };
            var cacheKey = new CacheKey(key, tags);
            cacheKey.Should().NotBeNull();

            var absoluteExpiration = DateTimeOffset.Now.AddMinutes(5);
            var cachePolicy = CachePolicy.WithAbsoluteExpiration(absoluteExpiration);
            cachePolicy.Should().NotBeNull();

            var policy = MemoryCacheProvider.CreatePolicy(cacheKey, cachePolicy);
            policy.Should().NotBeNull();
            policy.AbsoluteExpiration.Should().Be(absoluteExpiration);
            policy.ChangeMonitors.Should().NotBeNull();
            policy.ChangeMonitors.Should().HaveCount(1);
            policy.ChangeMonitors.Should().ContainItemsAssignableTo<CacheEntryChangeMonitor>();
        }

        [Fact]
        public void CreatePolicySlidingTest()
        {
            string key = DateTime.Now.Ticks.ToString(CultureInfo.InvariantCulture);
            string[] tags = new[] { "a", "b" };
            var cacheKey = new CacheKey(key, tags);
            cacheKey.Should().NotBeNull();

            var slidingExpiration = TimeSpan.FromMinutes(5);
            var cachePolicy = CachePolicy.WithSlidingExpiration(slidingExpiration);
            cachePolicy.Should().NotBeNull();

            var policy = MemoryCacheProvider.CreatePolicy(cacheKey, cachePolicy);
            policy.Should().NotBeNull();
            policy.SlidingExpiration.Should().Be(slidingExpiration);
            policy.ChangeMonitors.Should().NotBeNull();
            policy.ChangeMonitors.Should().HaveCount(1);
            policy.ChangeMonitors.Should().ContainItemsAssignableTo<CacheEntryChangeMonitor>();
        }

    }
}
