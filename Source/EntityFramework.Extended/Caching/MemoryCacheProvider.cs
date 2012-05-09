using System;
using System.Linq;
using System.Runtime.Caching;

namespace EntityFramework.Caching
{
    public class MemoryCacheProvider : ICacheProvider
    {
        private const string _tagKey = "global::tag::{0}";

        public bool Add(CacheKey cacheKey, object value, CachePolicy cachePolicy)
        {
            string key = GetKey(cacheKey);
            var item = new CacheItem(key, value);
            var policy = CreatePolicy(cacheKey, cachePolicy);

            return MemoryCache.Default.Add(item, policy);
        }
        
        public object Get(CacheKey cacheKey)
        {
            string key = GetKey(cacheKey);
            return MemoryCache.Default.Get(key);
        }

        public object GetOrAdd(CacheKey cacheKey, Func<CacheKey, object> valueFactory, CachePolicy cachePolicy)
        {
            string key = GetKey(cacheKey);
            if (MemoryCache.Default.Contains(key))
                return MemoryCache.Default.Get(key);

            // get value and add to cache
            object value = valueFactory(cacheKey);
            if (Add(cacheKey, value, cachePolicy))
                return value;

            // add failed
            return null;
        }

        public object Remove(CacheKey cacheKey)
        {
            string key = GetKey(cacheKey);
            return MemoryCache.Default.Remove(key);
        }

        public int Expire(CacheTag cacheTag)
        {
            string key = string.Format(_tagKey, cacheTag);
            var item = new CacheItem(key, DateTimeOffset.UtcNow.Ticks);
            var policy = new CacheItemPolicy { AbsoluteExpiration = ObjectCache.InfiniteAbsoluteExpiration };

            MemoryCache.Default.Set(item, policy);
            return 0;
        }

        public bool Set(CacheKey cacheKey, object value, CachePolicy cachePolicy)
        {
            string key = GetKey(cacheKey);
            var item = new CacheItem(key, value);
            var policy = CreatePolicy(cacheKey, cachePolicy);

            MemoryCache.Default.Set(item, policy);
            return true;
        }
        

        private static string GetKey(CacheKey cacheKey)
        {
            return cacheKey.Key;
        }

        private static CacheItemPolicy CreatePolicy(CacheKey key, CachePolicy cachePolicy)
        {
            var policy = new CacheItemPolicy();

            switch (cachePolicy.Mode)
            {
                case CacheExpirationMode.Sliding:
                    policy.SlidingExpiration = cachePolicy.SlidingExpiration;
                    break;
                case CacheExpirationMode.Absolute:
                    policy.AbsoluteExpiration = cachePolicy.AbsoluteExpiration;
                    break;
                default:
                    policy.AbsoluteExpiration = ObjectCache.InfiniteAbsoluteExpiration;
                    break;
            }

            var changeMonitor = CreateChangeMonitor(key);
            if (changeMonitor != null)
                policy.ChangeMonitors.Add(changeMonitor);

            return policy;
        }

        private static CacheEntryChangeMonitor CreateChangeMonitor(CacheKey key)
        {
            var cache = MemoryCache.Default;

            var tags = key.Tags
                .Select(t => string.Format(_tagKey, (object)t))
                .ToList();

            if (tags.Count == 0)
                return null;

            // make sure tags exist
            foreach (string tag in tags)
                cache.AddOrGetExisting(tag, DateTimeOffset.UtcNow.Ticks, ObjectCache.InfiniteAbsoluteExpiration);

            return cache.CreateCacheEntryChangeMonitor(tags);
        }

    }
}