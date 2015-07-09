using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EntityFramework.Extensions
{
    /// <summary>
    /// A generic interface for injecting query unwrappers.
    /// </summary>
    public interface IQueryUnwrapper
    {
        /// <summary>
        /// Unwrap the given query
        /// </summary>
        /// <typeparam name="TEntity">the entity type</typeparam>
        /// <param name="query">the query to unwrap</param>
        /// <returns>the unwrapped query</returns>
        IQueryable<TEntity> Unwrap<TEntity>(IQueryable<TEntity> query)
            where TEntity : class;
    }
}
