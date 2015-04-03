using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EntityFramework.Caching
{
    /// <summary>
    /// The CacheManager with support for providers used by FromCache extension methods.
    /// </summary>
    /// <remarks>
    /// <para>
    /// This CacheManager uses the <see cref="Locator"/> container to resolve an <see cref="ICacheProvider"/> to store the cache items.
    /// </para>
    /// <para>
    /// The CacheManager also supports tagging the cache entry to support expiring by tag. <see cref="CacheKey"/> supports a list
    /// of tags to associate with the cache entry.  Use <see cref="F:Expire"/> to evict the cache entry by <see cref="CacheTag"/>.
    /// </para>
    /// </remarks>
    /// <example>
    /// Replace cache provider with Memcached provider
    /// <code><![CDATA[
    /// Locator.Current.Register<ICacheProvider>(() => new MemcachedProvider());
    /// ]]>
    /// </code>
    /// </example>
    public class CacheManager
    {
        private static readonly Lazy<CacheManager> _current = new Lazy<CacheManager>(() => new CacheManager());

        /// <summary>
        /// Gets the current singleton instance of <see cref="CacheManager"/>.
        /// </summary>
        /// <value>The current singleton instance.</value>
        public static CacheManager Current
        {
            get { return _current.Value; }
        }

        /// <summary>
        /// Inserts a cache entry into the cache without overwriting any existing cache entry.
        /// </summary>
        /// <param name="key">A unique identifier for the cache entry.</param>
        /// <param name="value">The object to insert.</param>
        /// <returns><c>true</c> if insertion succeeded, or <c>false</c> if there is an already an entry in the cache that has the same key as key.</returns>
        public bool Add(string key, object value)
        {
            var cachePolicy = new CachePolicy();
            return Add(key, value, cachePolicy);
        }

        /// <summary>
        /// Inserts a cache entry into the cache without overwriting any existing cache entry.
        /// </summary>
        /// <param name="key">A unique identifier for the cache entry.</param>
        /// <param name="value">The object to insert.</param>
        /// <param name="absoluteExpiration">The fixed date and time at which the cache entry will expire.</param>
        /// <returns>
        ///   <c>true</c> if insertion succeeded, or <c>false</c> if there is an already an entry in the cache that has the same key as key.
        /// </returns>
        public bool Add(string key, object value, DateTimeOffset absoluteExpiration)
        {
            var cachePolicy = CachePolicy.WithAbsoluteExpiration(absoluteExpiration);
            return Add(key, value, cachePolicy);
        }

        /// <summary>
        /// Inserts a cache entry into the cache without overwriting any existing cache entry.
        /// </summary>
        /// <param name="key">A unique identifier for the cache entry.</param>
        /// <param name="value">The object to insert.</param>
        /// <param name="slidingExpiration">A span of time within which a cache entry must be accessed before the cache entry is evicted from the cache.</param>
        /// <returns>
        ///   <c>true</c> if insertion succeeded, or <c>false</c> if there is an already an entry in the cache that has the same key as key.
        /// </returns>
        public bool Add(string key, object value, TimeSpan slidingExpiration)
        {
            var cachePolicy = CachePolicy.WithSlidingExpiration(slidingExpiration);
            return Add(key, value, cachePolicy);
        }

        /// <summary>
        /// Inserts a cache entry into the cache without overwriting any existing cache entry.
        /// </summary>
        /// <param name="key">A unique identifier for the cache entry.</param>
        /// <param name="value">The object to insert.</param>
        /// <param name="cachePolicy">An object that contains eviction details for the cache entry.</param>
        /// <returns>
        ///   <c>true</c> if insertion succeeded, or <c>false</c> if there is an already an entry in the cache that has the same key as key.
        /// </returns>
        public bool Add(string key, object value, CachePolicy cachePolicy)
        {
            var cacheKey = new CacheKey(key);
            return Add(cacheKey, value, cachePolicy);
        }

        /// <summary>
        /// Inserts a cache entry into the cache without overwriting any existing cache entry.
        /// </summary>
        /// <param name="cacheKey">A unique identifier for the cache entry.</param>
        /// <param name="value">The object to insert.</param>
        /// <param name="cachePolicy">An object that contains eviction details for the cache entry.</param>
        /// <returns>
        ///   <c>true</c> if insertion succeeded, or <c>false</c> if there is an already an entry in the cache that has the same key as key.
        /// </returns>
        public virtual bool Add(CacheKey cacheKey, object value, CachePolicy cachePolicy)
        {
            var provider = ResolveProvider();
            return provider.Add(cacheKey, value, cachePolicy);
        }


        /// <summary>
        /// Gets the cache value for the specified key
        /// </summary>
        /// <param name="key">A unique identifier for the cache entry.</param>
        /// <returns>The cache value for the specified key, if the entry exists; otherwise, <see langword="null"/>.</returns>
        public virtual object Get(string key)
        {
            var cacheKey = new CacheKey(key);

            var provider = ResolveProvider();
            var item = provider.Get(cacheKey);

            return item;
        }


        /// <summary>
        /// Gets the cache value for the specified key that is already in the dictionary or the new value if the key was not in the dictionary.
        /// </summary>
        /// <param name="key">A unique identifier for the cache entry.</param>
        /// <param name="value">The object to insert.</param>
        /// <returns>
        /// The value for the key. This will be either the existing value for the key if the key is already in the dictionary, 
        /// or the new value if the key was not in the dictionary.
        /// </returns>
        public object GetOrAdd(string key, object value)
        {
            var policy = new CachePolicy();
            return GetOrAdd(key, value, policy);
        }

        /// <summary>
        /// Gets the cache value for the specified key that is already in the dictionary or the new value if the key was not in the dictionary.
        /// </summary>
        /// <param name="key">A unique identifier for the cache entry.</param>
        /// <param name="value">The object to insert.</param>
        /// <param name="absoluteExpiration">The fixed date and time at which the cache entry will expire.</param>
        /// <returns>
        /// The value for the key. This will be either the existing value for the key if the key is already in the dictionary, 
        /// or the new value if the key was not in the dictionary.
        /// </returns>
        public object GetOrAdd(string key, object value, DateTimeOffset absoluteExpiration)
        {
            var policy = CachePolicy.WithAbsoluteExpiration(absoluteExpiration);
            return GetOrAdd(key, value, policy);
        }

        /// <summary>
        /// Gets the cache value for the specified key that is already in the dictionary or the new value if the key was not in the dictionary.
        /// </summary>
        /// <param name="key">A unique identifier for the cache entry.</param>
        /// <param name="value">The object to insert.</param>
        /// <param name="slidingExpiration">A span of time within which a cache entry must be accessed before the cache entry is evicted from the cache.</param>
        /// <returns>
        /// The value for the key. This will be either the existing value for the key if the key is already in the dictionary, 
        /// or the new value if the key was not in the dictionary.
        /// </returns>
        public object GetOrAdd(string key, object value, TimeSpan slidingExpiration)
        {
            var policy = CachePolicy.WithSlidingExpiration(slidingExpiration);
            return GetOrAdd(key, value, policy);
        }

        /// <summary>
        /// Gets the cache value for the specified key that is already in the dictionary or the new value if the key was not in the dictionary.
        /// </summary>
        /// <param name="key">A unique identifier for the cache entry.</param>
        /// <param name="value">The object to insert.</param>
        /// <param name="cachePolicy">An object that contains eviction details for the cache entry.</param>
        /// <returns>
        /// The value for the key. This will be either the existing value for the key if the key is already in the dictionary, 
        /// or the new value if the key was not in the dictionary.
        /// </returns>
        public object GetOrAdd(string key, object value, CachePolicy cachePolicy)
        {
            var cacheKey = new CacheKey(key);
            return GetOrAdd(cacheKey, value, cachePolicy);
        }

        /// <summary>
        /// Gets the cache value for the specified key that is already in the dictionary or the new value if the key was not in the dictionary.
        /// </summary>
        /// <param name="key">A unique identifier for the cache entry.</param>
        /// <param name="value">The object to insert.</param>
        /// <param name="cachePolicy">An object that contains eviction details for the cache entry.</param>
        /// <returns>
        /// The value for the key. This will be either the existing value for the key if the key is already in the dictionary, 
        /// or the new value if the key was not in the dictionary.
        /// </returns>
        public object GetOrAdd(CacheKey key, object value, CachePolicy cachePolicy)
        {
            return GetOrAdd(key, k => value, cachePolicy);
        }


        /// <summary>
        /// Gets the cache value for the specified key that is already in the dictionary or the new value for the key as returned by <paramref name="valueFactory"/>.
        /// </summary>
        /// <param name="key">A unique identifier for the cache entry.</param>
        /// <param name="valueFactory">The function used to generate a value to insert into cache.</param>
        /// <returns>
        /// The value for the key. This will be either the existing value for the key if the key is already in the dictionary, 
        /// or the new value for the key as returned by <paramref name="valueFactory"/> if the key was not in the dictionary.
        /// </returns>
        public object GetOrAdd(string key, Func<string, object> valueFactory)
        {
            return GetOrAdd(key, valueFactory, new CachePolicy());
        }

        /// <summary>
        /// Gets the cache value for the specified key that is already in the dictionary or the new value for the key as returned by <paramref name="valueFactory"/>.
        /// </summary>
        /// <param name="key">A unique identifier for the cache entry.</param>
        /// <param name="valueFactory">The function used to generate a value to insert into cache.</param>
        /// <param name="absoluteExpiration">The fixed date and time at which the cache entry will expire.</param>
        /// <returns>
        /// The value for the key. This will be either the existing value for the key if the key is already in the dictionary, 
        /// or the new value for the key as returned by <paramref name="valueFactory"/> if the key was not in the dictionary.
        /// </returns>
        public object GetOrAdd(string key, Func<string, object> valueFactory, DateTimeOffset absoluteExpiration)
        {
            var policy = CachePolicy.WithAbsoluteExpiration(absoluteExpiration);
            return GetOrAdd(key, valueFactory, policy);
        }

        /// <summary>
        /// Gets the cache value for the specified key that is already in the dictionary or the new value for the key as returned by <paramref name="valueFactory"/>.
        /// </summary>
        /// <param name="key">A unique identifier for the cache entry.</param>
        /// <param name="valueFactory">The function used to generate a value to insert into cache.</param>
        /// <param name="slidingExpiration">A span of time within which a cache entry must be accessed before the cache entry is evicted from the cache.</param>
        /// <returns>
        /// The value for the key. This will be either the existing value for the key if the key is already in the dictionary, 
        /// or the new value for the key as returned by <paramref name="valueFactory"/> if the key was not in the dictionary.
        /// </returns>
        public object GetOrAdd(string key, Func<string, object> valueFactory, TimeSpan slidingExpiration)
        {
            var policy = CachePolicy.WithSlidingExpiration(slidingExpiration);
            return GetOrAdd(key, valueFactory, policy);
        }

        /// <summary>
        /// Gets the cache value for the specified key that is already in the dictionary or the new value for the key as returned by <paramref name="valueFactory"/>.
        /// </summary>
        /// <param name="key">A unique identifier for the cache entry.</param>
        /// <param name="valueFactory">The function used to generate a value to insert into cache.</param>
        /// <param name="cachePolicy">An object that contains eviction details for the cache entry.</param>
        /// <returns>
        /// The value for the key. This will be either the existing value for the key if the key is already in the dictionary, 
        /// or the new value for the key as returned by <paramref name="valueFactory"/> if the key was not in the dictionary.
        /// </returns>
        public object GetOrAdd(string key, Func<string, object> valueFactory, CachePolicy cachePolicy)
        {
            var cacheKey = new CacheKey(key);
            return GetOrAdd(cacheKey, valueFactory, cachePolicy);
        }

        /// <summary>
        /// Gets the cache value for the specified key that is already in the dictionary or the new value for the key as returned by <paramref name="valueFactory"/>.
        /// </summary>
        /// <param name="cacheKey">A unique identifier for the cache entry.</param>
        /// <param name="valueFactory">The function used to generate a value to insert into cache.</param>
        /// <param name="cachePolicy">An object that contains eviction details for the cache entry.</param>
        /// <returns>
        /// The value for the key. This will be either the existing value for the key if the key is already in the dictionary, 
        /// or the new value for the key as returned by <paramref name="valueFactory"/> if the key was not in the dictionary.
        /// </returns>
        public virtual object GetOrAdd(CacheKey cacheKey, Func<CacheKey, object> valueFactory, CachePolicy cachePolicy)
        {
            var provider = ResolveProvider();
            var item = provider.GetOrAdd(cacheKey, valueFactory, cachePolicy);

            return item;
        }

#if NET45
        /// <summary>
        /// Gets the cache value for the specified key that is already in the dictionary or the new value for the key as returned asynchronously by <paramref name="valueFactory"/>.
        /// </summary>
        /// <param name="cacheKey">A unique identifier for the cache entry.</param>
        /// <param name="valueFactory">The asynchronous function used to generate a value to insert into cache.</param>
        /// <param name="cachePolicy">An object that contains eviction details for the cache entry.</param>
        /// <returns>
        /// The value for the key. This will be either the existing value for the key if the key is already in the dictionary, 
        /// or the new value for the key as returned by <paramref name="valueFactory"/> if the key was not in the dictionary.
        /// </returns>
        public virtual Task<object> GetOrAddAsync(CacheKey cacheKey, Func<CacheKey, Task<object>> valueFactory, CachePolicy cachePolicy)
        {
            var provider = ResolveProvider();
            var item = provider.GetOrAddAsync(cacheKey, valueFactory, cachePolicy);

            return item;
        }
#endif

        /// <summary>
        /// Removes a cache entry from the cache. 
        /// </summary>
        /// <param name="key">A unique identifier for the cache entry.</param>
        /// <returns>If the entry is found in the cache, the removed cache entry; otherwise, <see langword="null"/>.</returns>
        public object Remove(string key)
        {
            var cacheKey = new CacheKey(key);
            return Remove(cacheKey);
        }

        /// <summary>
        /// Removes a cache entry from the cache. 
        /// </summary>
        /// <param name="cacheKey">A unique identifier for the cache entry.</param>
        /// <returns>If the entry is found in the cache, the removed cache entry; otherwise, <see langword="null"/>.</returns>
        public virtual object Remove(CacheKey cacheKey)
        {
            var provider = ResolveProvider();
            var item = provider.Remove(cacheKey);

            return item;
        }


        /// <summary>
        /// Expires the specified cache tag.
        /// </summary>
        /// <param name="tag">The cache tag.</param>
        /// <returns></returns>
        public int Expire(string tag)
        {
            var cacheTag = new CacheTag(tag);
            return Expire(cacheTag);
        }

        /// <summary>
        /// Expires the specified cache tag.
        /// </summary>
        /// <param name="cacheTag">The cache tag.</param>
        /// <returns></returns>
        public virtual int Expire(CacheTag cacheTag)
        {
            var provider = ResolveProvider();
            var item = provider.Expire(cacheTag);

            return item;
        }


        /// <summary>
        /// Inserts a cache entry into the cache overwriting any existing cache entry.
        /// </summary>
        /// <param name="key">A unique identifier for the cache entry.</param>
        /// <param name="value">The object to insert.</param>
        public void Set(string key, object value)
        {
            var policy = new CachePolicy();
            Set(key, value, policy);
        }

        /// <summary>
        /// Inserts a cache entry into the cache overwriting any existing cache entry.
        /// </summary>
        /// <param name="key">A unique identifier for the cache entry.</param>
        /// <param name="value">The object to insert.</param>
        /// <param name="absoluteExpiration">The fixed date and time at which the cache entry will expire.</param>
        public void Set(string key, object value, DateTimeOffset absoluteExpiration)
        {
            var policy = CachePolicy.WithAbsoluteExpiration(absoluteExpiration);
            Set(key, value, policy);
        }

        /// <summary>
        /// Inserts a cache entry into the cache overwriting any existing cache entry.
        /// </summary>
        /// <param name="key">A unique identifier for the cache entry.</param>
        /// <param name="value">The object to insert.</param>
        /// <param name="slidingExpiration">A span of time within which a cache entry must be accessed before the cache entry is evicted from the cache.</param>
        public void Set(string key, object value, TimeSpan slidingExpiration)
        {
            var policy = CachePolicy.WithSlidingExpiration(slidingExpiration);
            Set(key, value, policy);
        }

        /// <summary>
        /// Inserts a cache entry into the cache overwriting any existing cache entry.
        /// </summary>
        /// <param name="key">A unique identifier for the cache entry.</param>
        /// <param name="value">The object to insert.</param>
        /// <param name="cachePolicy">An object that contains eviction details for the cache entry.</param>
        public void Set(string key, object value, CachePolicy cachePolicy)
        {
            var cacheKey = new CacheKey(key);
            Set(cacheKey, value, cachePolicy);
        }

        /// <summary>
        /// Inserts a cache entry into the cache overwriting any existing cache entry.
        /// </summary>
        /// <param name="cacheKey">A unique identifier for the cache entry.</param>
        /// <param name="value">The object to insert.</param>
        /// <param name="cachePolicy">An object that contains eviction details for the cache entry.</param>
        public virtual void Set(CacheKey cacheKey, object value, CachePolicy cachePolicy)
        {
            var provider = ResolveProvider();
            provider.Set(cacheKey, value, cachePolicy);
        }

        /// <summary>
        /// Clears all entries from the cache
        /// </summary>
        public virtual void Clear()
        {
            var provider = ResolveProvider();
            provider.ClearCache();
        }


        /// <summary>
        /// Gets the registered <see cref="ICacheProvider"/> from <see cref="Locator"/>.
        /// </summary>
        /// <returns>An instance from <see cref="Locator"/>.</returns>
        protected ICacheProvider ResolveProvider()
        {
            var provider = Locator.Current.Resolve<ICacheProvider>();
            if (provider == null)
                throw new InvalidOperationException("Could not resolve the ICacheProvider. Make sure ICacheProvider is registered in the Locator.Current container.");

            return provider;
        }
    }
}
