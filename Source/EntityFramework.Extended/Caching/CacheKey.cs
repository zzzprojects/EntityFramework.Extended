using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace EntityFramework.Caching
{
    /// <summary>
    /// A class representing a unique key for cache items.
    /// </summary>
    public class CacheKey
    {
        private readonly string _key;
        private readonly HashSet<CacheTag> _tags;

        /// <summary>
        /// Initializes a new instance of the <see cref="CacheKey"/> class.
        /// </summary>
        /// <param name="key">The key for a cache item.</param>
        public CacheKey(string key)
            : this(key, Enumerable.Empty<string>())
        { }

        /// <summary>
        /// Initializes a new instance of the <see cref="CacheKey"/> class.
        /// </summary>
        /// <param name="key">The key for a cache item.</param>
        /// <param name="tags">The tags for the cache item.</param>
        public CacheKey(string key, IEnumerable<string> tags)
        {
            if (key == null)
                throw new ArgumentNullException("key");
            if (tags == null)
                throw new ArgumentNullException("tags");

            _key = key;

            var cacheTags = tags.Select(k => new CacheTag(k));
            _tags = new HashSet<CacheTag>(cacheTags);
        }


        /// <summary>
        /// Gets the key for a cached item.
        /// </summary>
        public string Key
        {
            get { return _key; }
        }

        /// <summary>
        /// Gets the tags for a cached item.
        /// </summary>
        public HashSet<CacheTag> Tags
        {
            get { return _tags; }
        }
    }
}
