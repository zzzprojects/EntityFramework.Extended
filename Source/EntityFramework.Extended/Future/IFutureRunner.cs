using System;
using System.Collections.Generic;
using System.Data.Entity.Core.Objects;
using System.Threading;
using System.Threading.Tasks;

namespace EntityFramework.Future
{
    /// <summary>
    /// A interface encapsulating the execution of future queries.
    /// </summary>
    public interface IFutureRunner
    {
        /// <summary>
        /// Executes the future queries.
        /// </summary>
        /// <param name="context">The <see cref="ObjectContext"/> to run the queries against.</param>
        /// <param name="futureQueries">The future queries list.</param>
        void ExecuteFutureQueries(ObjectContext context, IList<IFutureQuery> futureQueries);

#if NET45
        /// <summary>
        /// Executes the future queries.
        /// </summary>
        /// <param name="context">The <see cref="ObjectContext" /> to run the queries against.</param>
        /// <param name="futureQueries">The future queries list.</param>
        /// <param name="cancellationToken">The cancellation token.</param>
        /// <returns></returns>
        Task ExecuteFutureQueriesAsync(ObjectContext context, IList<IFutureQuery> futureQueries, CancellationToken cancellationToken);
#endif
    }
}