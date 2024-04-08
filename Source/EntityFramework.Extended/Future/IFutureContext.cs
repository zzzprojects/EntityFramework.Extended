using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace EntityFramework.Future
{
    /// <summary>
    /// An interface defining storage for future queries.
    /// </summary>
    public interface IFutureContext
    {
        /// <summary>
        /// Gets the future queries.
        /// </summary>
        /// <value>The future queries.</value>
        IList<IFutureQuery> FutureQueries { get; }

        /// <summary>
        /// Executes the future queries.
        /// </summary>
        void ExecuteFutureQueries();

#if NET45
        /// <summary>
        /// Executes the future queries.
        /// </summary>
        /// <param name="cancellationToken">The cancellation token.</param>
        /// <returns></returns>
        Task ExecuteFutureQueriesAsync(CancellationToken cancellationToken);
#endif

        /// <summary>
        /// Adds the future query to the waiting queries list on this context.
        /// </summary>
        /// <param name="query">The future query.</param>
        void AddQuery(IFutureQuery query);
    }
}