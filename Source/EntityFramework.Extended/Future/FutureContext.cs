using System;
using System.Collections.Generic;
using System.Data.Entity.Core.Objects;
using System.Threading;
using System.Threading.Tasks;

namespace EntityFramework.Future
{
    /// <summary>
    /// A class to hold waiting future queries for an <see cref="ObjectContext"/>.
    /// </summary>
    /// <remarks>
    /// This class creates a link between the <see cref="ObjectContext"/> and
    /// the waiting future queries. Since the ObjectContext could
    /// be displosed before this class, ObjectContext is stored as
    /// a <see cref="WeakReference"/>. 
    /// </remarks>
    public class FutureContext : IFutureContext
    {
        private readonly IList<IFutureQuery> _futureQueries;
        private readonly WeakReference _objectContext;

        /// <summary>
        /// Initializes a new instance of the <see cref="FutureContext"/> class.
        /// </summary>
        /// <param name="objectContext">The object context for the future queries.</param>
        public FutureContext(ObjectContext objectContext)
        {
            if (objectContext == null)
                throw new ArgumentNullException("objectContext");

            _objectContext = new WeakReference(objectContext);
            _futureQueries = new List<IFutureQuery>();
        }

        /// <summary>
        /// Gets the future queries.
        /// </summary>
        /// <value>
        /// The future queries.
        /// </value>
        public IList<IFutureQuery> FutureQueries
        {
            get { return _futureQueries; }
        }

        /// <summary>
        /// Gets the <see cref="ObjectContext"/> for the FutureQueries.
        /// </summary>
        /// <remarks>
        /// ObjectContext is stored as a WeakReference.  The value can be disposed.
        /// </remarks>
        public ObjectContext ObjectContext
        {
            get
            {
                return _objectContext.IsAlive
                  ? _objectContext.Target as ObjectContext
                  : null;
            }
        }

        /// <summary>
        /// Gets an indication whether the object referenced ObjectContext has been garbage collected.
        /// </summary>
        /// <value>
        ///   <c>true</c> if this instance is alive; otherwise, <c>false</c>.
        /// </value>
        public bool IsAlive
        {
            get { return _objectContext.IsAlive; }
        }

        /// <summary>
        /// Executes the future queries as a single batch.
        /// </summary>
        public void ExecuteFutureQueries()
        {
            ObjectContext context = ObjectContext;
            if (context == null)
                throw new ObjectDisposedException("ObjectContext", "The ObjectContext for the future queries has been displosed.");

            var runner = Locator.Current.Resolve<IFutureRunner>();
            if (runner == null)
                throw new InvalidOperationException("Could not resolve the IFutureRunner. Make sure IFutureRunner is registered in the Locator.Current container.");

            runner.ExecuteFutureQueries(context, FutureQueries);
        }

#if NET45
        /// <summary>
        /// Executes the future queries as a single batch.
        /// </summary>
        public async Task ExecuteFutureQueriesAsync(CancellationToken cancellationToken = default(CancellationToken))
            {
            ObjectContext context = ObjectContext;
            if (context == null)
                throw new ObjectDisposedException("ObjectContext", "The ObjectContext for the future queries has been disposed.");

            var runner = Locator.Current.Resolve<IFutureRunner>();
            if (runner == null)
                throw new InvalidOperationException("Could not resolve the IFutureRunner. Make sure IFutureRunner is registered in the Locator.Current container.");

            await runner.ExecuteFutureQueriesAsync(context, FutureQueries, cancellationToken).ConfigureAwait(false);
            }
#endif

        /// <summary>
        /// Adds the future query to the waiting queries list on this context.
        /// </summary>
        /// <param name="query">The future query.</param>
        public void AddQuery(IFutureQuery query)
        {
            if (query == null)
                throw new ArgumentNullException("query");

            FutureQueries.Add(query);
        }
    }
}
