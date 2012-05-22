using System;
using System.Collections.Generic;
using System.Linq;
using System.Data.Entity;
using System.Text;
using EntityFramework.Caching;

namespace EntityFramework.Extensions
{
    /// <summary>
    /// Extension methods for query cache.
    /// </summary>
    public static class CacheExtensions
    {
        /// <summary>
        /// Returns the result of the <paramref name="query"/>; if possible from the cache,
        /// otherwise the query is materialized and the result cached before being returned.
        /// </summary>
        /// <typeparam name="TEntity">The type of the data in the data source.</typeparam>
        /// <param name="query">The query to be materialized.</param>
        /// <param name="cachePolicy">The cache policy for the query.</param>
        /// <param name="tags">The list of tags to use for cache expiration.</param>
        /// <returns>
        /// The result of the query.
        /// </returns>
        public static IEnumerable<TEntity> FromCache<TEntity>(this IQueryable<TEntity> query, CachePolicy cachePolicy = null, IEnumerable<string> tags = null)
            where TEntity : class
        {
            string key = query.GetCacheKey();
            var cacheKey = new CacheKey(key,
                tags ?? Enumerable.Empty<string>());

            // allow override of CacheManager
            var manager = Locator.Current.Resolve<CacheManager>();

            var result = manager.GetOrAdd(
                cacheKey,
                k => query.AsNoTracking().ToList(),
                cachePolicy ?? CachePolicy.Default
            ) as IEnumerable<TEntity>;

            return result;
        }

        /// <summary>
        /// Returns the first element of the <paramref name="query"/>; if possible from the cache,
        /// otherwise the query is materialized and the result cached before being returned.
        /// </summary>
        /// <typeparam name="TEntity">The type of the data in the data source.</typeparam>
        /// <param name="query">The query to be materialized.</param>
        /// <param name="cachePolicy">The cache policy for the query.</param>
        /// <param name="tags">The list of tags to use for cache expiration.</param>
        /// <returns>default(T) if source is empty; otherwise, the first element in source.</returns>
        public static TEntity FromCacheFirstOrDefault<TEntity>(this IQueryable<TEntity> query, CachePolicy cachePolicy = null, IEnumerable<string> tags = null)
            where TEntity : class
        {
            return query
                .Take(1)
                .FromCache(cachePolicy, tags)
                .FirstOrDefault();
        }

        /// <summary>
        /// Removes the cached query from cache.
        /// </summary>
        /// <typeparam name="TEntity">The type of the data in the data source.</typeparam>
        /// <param name="query">The query to be materialized.</param>
        /// <returns>
        /// The original <paramref name="query"/> for fluent chaining.
        /// </returns>
        public static IQueryable<TEntity> RemoveCache<TEntity>(this IQueryable<TEntity> query)
            where TEntity : class
        {
            IEnumerable<TEntity> removed;
            return RemoveCache(query, out removed);
        }

        /// <summary>
        /// Removes the cached query from cache.
        /// </summary>
        /// <typeparam name="TEntity">The type of the data in the data source.</typeparam>
        /// <param name="query">The query to be materialized.</param>
        /// <param name="removed">The removed items for cache.</param>
        /// <returns>
        /// The original <paramref name="query"/> for fluent chaining.
        /// </returns>
        public static IQueryable<TEntity> RemoveCache<TEntity>(this IQueryable<TEntity> query, out IEnumerable<TEntity> removed)
            where TEntity : class
        {
            string key = query.GetCacheKey();
            
            // allow override of CacheManager
            var manager = Locator.Current.Resolve<CacheManager>();

            removed = manager.Remove(key) as IEnumerable<TEntity>;
            return query;
        }
    }
}
