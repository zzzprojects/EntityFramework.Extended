using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Objects;

namespace EntityFramework.Future
{
    /// <summary>
    /// A class to hold waiting future queries for an ObjectContext.
    /// </summary>
    /// <remarks>
    /// This class creates a link between the ObjectContext and
    /// the waiting future queries. Since the ObjectContext could
    /// be displosed before this class, ObjectContext is stored as
    /// a WeakReference. 
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

            FutureRunner.ExecuteFutureQueries(context, FutureQueries);
        }

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
