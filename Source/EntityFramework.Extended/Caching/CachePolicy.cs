using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Caching;
using System.Text;

namespace EntityFramework.Caching
{
    /// <summary>
    /// Represents a set of eviction and expiration details for a specific cache entry.
    /// </summary>
    public class CachePolicy
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="CachePolicy"/> class.
        /// </summary>
        public CachePolicy()
        {
            Mode = CacheExpirationMode.None;
            AbsoluteExpiration = ObjectCache.InfiniteAbsoluteExpiration;
            SlidingExpiration = ObjectCache.NoSlidingExpiration;
        }

        /// <summary>
        /// Gets or sets the cache expiration mode.
        /// </summary>
        /// <value>The cache expiration mode.</value>
        public CacheExpirationMode Mode { get; set; }

        /// <summary>
        /// Gets or sets a value that indicates whether a cache entry should be evicted after a specified duration.
        /// </summary>
        public DateTimeOffset AbsoluteExpiration { get; set; }

        /// <summary>
        /// Gets or sets a value that indicates whether a cache entry should be evicted if it has not been accessed in a given span of time. 
        /// </summary>
        public TimeSpan SlidingExpiration { get; set; }

        /// <summary>
        /// Creates a <see cref="CachePolicy"/> with the absolute expiration.
        /// </summary>
        /// <param name="absoluteExpiration">The absolute expiration.</param>
        /// <returns></returns>
        public static CachePolicy WithAbsoluteExpiration(DateTimeOffset absoluteExpiration)
        {
            var policy = new CachePolicy
            {
                Mode = CacheExpirationMode.Absolute,
                AbsoluteExpiration = absoluteExpiration
            };
            return policy;
        }

        /// <summary>
        /// Creates a <see cref="CachePolicy"/> with the sliding expiration.
        /// </summary>
        /// <param name="slidingExpiration">The sliding expiration.</param>
        /// <returns></returns>
        public static CachePolicy WithSlidingExpiration(TimeSpan slidingExpiration)
        {
            var policy = new CachePolicy
            {
                Mode = CacheExpirationMode.Sliding,
                SlidingExpiration = slidingExpiration
            };
            return policy;
        }
    }
}
