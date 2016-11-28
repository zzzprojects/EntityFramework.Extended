using System;
using System.Linq;

namespace EntityFramework
{
    /// <summary>
    /// A generic <see langword="interface"/> for unwrapping an underlying queries.
    /// </summary>
    public interface IQueryUnwrapper
    {
        /// <summary>
        /// Unwrap the specified <paramref name="query"/>.
        /// </summary>
        /// <typeparam name="TEntity">the entity type</typeparam>
        /// <param name="query">the query to unwrap</param>
        /// <returns>The unwrapped query</returns>
        IQueryable<TEntity> Unwrap<TEntity>(IQueryable<TEntity> query)
            where TEntity : class;
        
        /// <summary>
        /// Unwrap the specified <paramref name="query"/>.
        /// </summary>
        /// <param name="query">the query to unwrap</param>
        /// <returns>The unwrapped query</returns>
        IQueryable Unwrap(IQueryable query);
    }
}
