using System;
using System.Collections.Generic;
using System.Text;

namespace EntityFramework.Caching
{
    public interface ICacheProvider
    {
        bool Add(CacheKey cacheKey, object value, CachePolicy cachePolicy);

        object Get(CacheKey cacheKey);

        object GetOrAdd(CacheKey cacheKey, Func<CacheKey, object> valueFactory, CachePolicy cachePolicy);

        object Remove(CacheKey cacheKey);

        int Expire(CacheTag cacheTag);

        bool Set(CacheKey cacheKey, object value, CachePolicy cachePolicy);
    }
}
