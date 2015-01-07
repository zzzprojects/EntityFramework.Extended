using System.Linq;
using EntityFramework.Extensions;

namespace EntityFramework.Caching
{
    /// <summary>
    /// Extension methods for query cache.
    /// </summary>
    /// <remarks>
    /// Copyright (c) 2010 Pete Montgomery.
    /// http://petemontgomery.wordpress.com
    /// Licenced under GNU LGPL v3.
    /// http://www.gnu.org/licenses/lgpl.html
    /// </remarks>
    public static class QueryCache
    {
        /// <summary>
        /// Gets a cache key for the specified <see cref="IQueryable"/>.
        /// </summary>
        /// <param name="query">The query to get the key from.</param>
        /// <returns>A unique key for the specified <see cref="IQueryable"/></returns>
        public static string GetCacheKey<TEntity>(this IQueryable<TEntity> query) where TEntity : class
        {
            // The key is made up of the SQL that will be executed, along with any parameters and their values
            string key = query.ToTraceString();

            // the key is potentially very long, so use an md5 fingerprint
            // (fine if the query result data isn't critically sensitive)
            key = key.ToMd5Fingerprint();

            return key;
        }
    }
}
