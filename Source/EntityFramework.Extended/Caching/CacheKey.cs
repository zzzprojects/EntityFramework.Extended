using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace EntityFramework.Caching
{
    public class CacheKey
    {
        private readonly string _key;
        private readonly HashSet<CacheTag> _tags;

        public CacheKey(string key)
        {
            _key = key;
            _tags = new HashSet<CacheTag>();
        }

        public CacheKey(string key, IEnumerable<string> tags)
        {
            _key = key;

            var cacheTags = tags.Select(k => new CacheTag(k));
            _tags = new HashSet<CacheTag>(cacheTags);
        }


        public string Key
        {
            get { return _key; }
        }

        public HashSet<CacheTag> Tags
        {
            get { return _tags; }
        }
    }
}
