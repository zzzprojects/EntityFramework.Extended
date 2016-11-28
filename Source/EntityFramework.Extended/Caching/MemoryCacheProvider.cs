using System;
using System.Diagnostics;
using System.Linq;
using System.Runtime.Caching;
using System.Threading.Tasks;

namespace EntityFramework.Caching
{
    /// <summary>
    /// A cache provider based on <see cref="MemoryCache"/>.
    /// </summary>
    public class MemoryCacheProvider : ICacheProvider
    {
        private const string _tagKey = "global::tag::{0}";

        /// <summary>
        /// Inserts a cache entry into the cache without overwriting any existing cache entry.
        /// </summary>
        /// <param name="cacheKey">A unique identifier for the cache entry.</param>
        /// <param name="value">The object to insert.</param>
        /// <param name="cachePolicy">An object that contains eviction details for the cache entry.</param>
        /// <returns>
        ///   <c>true</c> if insertion succeeded, or <c>false</c> if there is an already an entry in the cache that has the same key as key.
        /// </returns>
        public bool Add(CacheKey cacheKey, object value, CachePolicy cachePolicy)
        {
            string key = GetKey(cacheKey);
            var item = new CacheItem(key, value);
            var policy = CreatePolicy(cacheKey, cachePolicy);

            var existing = MemoryCache.Default.AddOrGetExisting(item, policy);
            return existing.Value == null;
        }

        /// <summary>
        /// Gets the cache value for the specified key
        /// </summary>
        /// <param name="cacheKey">A unique identifier for the cache entry.</param>
        /// <returns>
        /// The cache value for the specified key, if the entry exists; otherwise, <see langword="null"/>.
        /// </returns>
        public object Get(CacheKey cacheKey)
        {
            string key = GetKey(cacheKey);
            return MemoryCache.Default.Get(key);
        }

        /// <summary>
        /// Gets the cache value for the specified key that is already in the dictionary or the new value for the key as returned by <paramref name="valueFactory"/>.
        /// </summary>
        /// <param name="cacheKey">A unique identifier for the cache entry.</param>
        /// <param name="valueFactory">The function used to generate a value to insert into cache.</param>
        /// <param name="cachePolicy">A <see cref="CachePolicy"/> that contains eviction details for the cache entry.</param>
        /// <returns>
        /// The value for the key. This will be either the existing value for the key if the key is already in the cache,
        /// or the new value for the key as returned by <paramref name="valueFactory"/> if the key was not in the cache.
        /// </returns>
        public object GetOrAdd(CacheKey cacheKey, Func<CacheKey, object> valueFactory, CachePolicy cachePolicy)
        {

            var key = GetKey(cacheKey);
            var cachedResult = MemoryCache.Default.Get(key);

            if (cachedResult != null)
            {
                Debug.WriteLine("Cache Hit : " + key);
                return cachedResult;
            }

            Debug.WriteLine("Cache Miss: " + key);

            // get value and add to cache, not bothered
            // if it succeeds or not just rerturn the value
            var value = valueFactory(cacheKey);
            this.Add(cacheKey, value, cachePolicy);

            return value;
        }

#if NET45
        /// <summary>
        /// Gets the cache value for the specified key that is already in the dictionary or the new value for the key as returned asynchronously by <paramref name="valueFactory"/>.
        /// </summary>
        /// <param name="cacheKey">A unique identifier for the cache entry.</param>
        /// <param name="valueFactory">The asynchronous function used to generate a value to insert into cache.</param>
        /// <param name="cachePolicy">A <see cref="CachePolicy"/> that contains eviction details for the cache entry.</param>
        /// <returns>
        /// The value for the key. This will be either the existing value for the key if the key is already in the cache,
        /// or the new value for the key as returned by <paramref name="valueFactory"/> if the key was not in the cache.
        /// </returns>
        public async Task<object> GetOrAddAsync(CacheKey cacheKey, Func<CacheKey, Task<object>> valueFactory, CachePolicy cachePolicy)
        {
            var key = GetKey(cacheKey);
            var cachedResult = MemoryCache.Default.Get(key);

            if (cachedResult != null)
            {
                Debug.WriteLine("Cache Hit : " + key);
                return cachedResult;
            }

            Debug.WriteLine("Cache Miss: " + key);

            // get value and add to cache, not bothered
            // if it succeeds or not just rerturn the value
            var value = await valueFactory(cacheKey).ConfigureAwait(false);
            this.Add(cacheKey, value, cachePolicy);

            return value;
        }
#endif

        /// <summary>
        /// Removes a cache entry from the cache.
        /// </summary>
        /// <param name="cacheKey">A unique identifier for the cache entry.</param>
        /// <returns>
        /// If the entry is found in the cache, the removed cache entry; otherwise, <see langword="null"/>.
        /// </returns>
        public object Remove(CacheKey cacheKey)
        {
            string key = GetKey(cacheKey);
            return MemoryCache.Default.Remove(key);
        }

        /// <summary>
        /// Expires the specified cache tag.
        /// </summary>
        /// <param name="cacheTag">The cache tag.</param>
        /// <returns>
        /// The number of items expired.
        /// </returns>
        public int Expire(CacheTag cacheTag)
        {
            string key = GetTagKey(cacheTag);
            var item = new CacheItem(key, DateTimeOffset.UtcNow.Ticks);
            var policy = new CacheItemPolicy { AbsoluteExpiration = ObjectCache.InfiniteAbsoluteExpiration };

            MemoryCache.Default.Set(item, policy);
            return 0;
        }

        /// <summary>
        /// Inserts a cache entry into the cache overwriting any existing cache entry.
        /// </summary>
        /// <param name="cacheKey">A unique identifier for the cache entry.</param>
        /// <param name="value">The object to insert.</param>
        /// <param name="cachePolicy">A <see cref="CachePolicy"/> that contains eviction details for the cache entry.</param>
        /// <returns></returns>
        public bool Set(CacheKey cacheKey, object value, CachePolicy cachePolicy)
        {
            string key = GetKey(cacheKey);
            var item = new CacheItem(key, value);
            var policy = CreatePolicy(cacheKey, cachePolicy);

            MemoryCache.Default.Set(item, policy);
            return true;
        }

        /// <summary>
        /// Clears all entries from the cache
        /// </summary>
        /// <returns></returns>
        public long ClearCache()
        {
            return MemoryCache.Default.Trim(100);
        }


        // internal for testing
        internal static string GetKey(CacheKey cacheKey)
        {
            return cacheKey.Key;
        }

        internal static string GetTagKey(CacheTag t)
        {
            return string.Format(_tagKey, t);
        }

        internal static CacheItemPolicy CreatePolicy(CacheKey key, CachePolicy cachePolicy)
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
                case CacheExpirationMode.Duration:
                    policy.AbsoluteExpiration = DateTimeOffset.Now.Add(cachePolicy.Duration);
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

        internal static CacheEntryChangeMonitor CreateChangeMonitor(CacheKey key)
        {
            var cache = MemoryCache.Default;

            var tags = key.Tags
                .Select(GetTagKey)
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