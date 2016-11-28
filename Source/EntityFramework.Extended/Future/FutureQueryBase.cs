using System;
using System.Collections.Generic;
using System.Data.Common;
using System.Data.Entity;
using System.Data.Entity.Core.Objects;
using System.Data.Entity.Infrastructure;
using System.Diagnostics;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using EntityFramework.Reflection;

namespace EntityFramework.Future
{
    /// <summary>
    /// Base class for future queries.
    /// </summary>
    /// <typeparam name="T">The type for the future query.</typeparam>
    [DebuggerDisplay("IsLoaded={IsLoaded}")]
    public abstract class FutureQueryBase<T> : IFutureQuery
    {
        private readonly IQueryable<T> _query;
        private readonly IFutureContext _futureContext;
        private IList<T> _result;
        private bool _isLoaded;

        /// <summary>
        /// Initializes a new instance of the <see cref="FutureQuery&lt;T&gt;" /> class.
        /// </summary>
        /// <param name="query">The query source to use when materializing.</param>
        /// <param name="futureContext">The future context.</param>
        protected FutureQueryBase(IQueryable<T> query, IFutureContext futureContext)
        {
            _query = query;
            _futureContext = futureContext;
            //_result = null;
        }

        /// <summary>
        /// Gets the action to execute when the query is accessed.
        /// </summary>
        /// <value>The load action.</value>
        //protected Action LoadAction
        //{
        //    get { return _loadAction; }
        //}

        /// <summary>
        /// Gets a value indicating whether this instance is loaded.
        /// </summary>
        /// <value><c>true</c> if this instance is loaded; otherwise, <c>false</c>.</value>
        public bool IsLoaded
        {
            get { return _isLoaded; }
        }

        /// <summary>
        /// Gets or sets the query execute exception. 
        /// </summary>
        /// <value>The query execute exception.</value>      
        public Exception Exception { get; set; }

        /// <summary>
        /// Gets the query source to use when materializing.
        /// </summary>
        /// <value>The query source to use when materializing.</value>
        IQueryable IFutureQuery.Query
        {
            get { return _query; }
        }

        /// <summary>
        /// Gets the result.
        /// </summary>
        /// <returns></returns>
        protected virtual IList<T> GetResult()
        {
            if (IsLoaded)
                return _result;

            // no load action, run query directly
            if (_futureContext == null)
            {
                _isLoaded = true;
                var enumerable = _query as IEnumerable<T>;
                _result = enumerable == null ? null : enumerable.ToList();
                return _result;
            }

            // invoke the load action on the datacontext
            // result will be set with a callback to SetResult
            _futureContext.ExecuteFutureQueries();
            return _result ?? new List<T>();
        }

#if NET45
        /// <summary>
        /// Gets the result asynchronous.
        /// </summary>
        /// <param name="cancellationToken">The cancellation token.</param>
        /// <returns></returns>
        protected virtual async Task<IList<T>> GetResultAsync(CancellationToken cancellationToken = default(CancellationToken))
        {
            if (IsLoaded)
                return _result;

            // no load action, run query directly
            if (_futureContext == null)
            {
                _isLoaded = true;
                _result = await (_query as IQueryable<T>).ToListAsync(cancellationToken).ConfigureAwait(false);
                return _result;
            }

            // invoke the load action on the datacontext
            // result will be set with a callback to SetResult
            await _futureContext.ExecuteFutureQueriesAsync(cancellationToken).ConfigureAwait(false);
            return _result ?? new List<T>();
        }
#endif

        /// <summary>
        /// Gets the data command for this query.
        /// </summary>
        /// <param name="dataContext">The data context to get the command from.</param>
        /// <returns>The requested command object.</returns>
        FuturePlan IFutureQuery.GetPlan(ObjectContext dataContext)
        {
            return GetPlan(dataContext);
        }

        /// <summary>
        /// Gets the data command for this query.
        /// </summary>
        /// <param name="dataContext">The data context to get the command from.</param>
        /// <returns>The requested command object.</returns>
        protected virtual FuturePlan GetPlan(ObjectContext dataContext)
        {
            IFutureQuery futureQuery = this;
            var source = futureQuery.Query;

            var q = source as ObjectQuery;
            if (q == null)
                throw new InvalidOperationException("The future query is not of type ObjectQuery.");

            var plan = new FuturePlan
            {
                CommandText = q.ToTraceString(),
                Parameters = q.Parameters
            };

            return plan;
        }

        /// <summary>
        /// Sets the underling value after the query has been executed.
        /// </summary>
        /// <param name="dataContext">The data context to translate the results with.</param>
        /// <param name="reader">The <see cref="DbDataReader"/> to get the result from.</param>
        void IFutureQuery.SetResult(ObjectContext dataContext, DbDataReader reader)
        {
            SetResult(dataContext, reader);
        }

        /// <summary>
        /// Sets the underling value after the query has been executed.
        /// </summary>
        /// <param name="dataContext">The data context to translate the results with.</param>
        /// <param name="reader">The <see cref="DbDataReader"/> to get the result from.</param>
        protected virtual void SetResult(ObjectContext dataContext, DbDataReader reader)
        {
            _isLoaded = true;

            try
            {
                IFutureQuery futureQuery = this;
                var source = futureQuery.Query;

                var q = source as ObjectQuery;
                if (q == null)
                    throw new InvalidOperationException("The future query is not of type ObjectQuery.");

                // create execution plan
                dynamic queryProxy = new DynamicProxy(q);
                // ObjectQueryState
                dynamic queryState = queryProxy.QueryState;


                // ObjectQueryExecutionPlan
                dynamic executionPlan = queryState.GetExecutionPlan(null);

                // ShaperFactory
                dynamic shaperFactory = executionPlan.ResultShaperFactory;
                // Shaper<T>
                dynamic shaper = shaperFactory.Create(reader, dataContext, dataContext.MetadataWorkspace,
                    MergeOption.AppendOnly, false, true, false);

                var list = new List<T>();
                IEnumerator<T> enumerator = shaper.GetEnumerator();
                while (enumerator.MoveNext())
                    list.Add(enumerator.Current);

                _result = list;

                // translate has issue with column names not matching
                //var resultSet = dataContext.Translate<T>(reader);
                //_result = resultSet.ToList();
            }
            catch (Exception ex)
            {
                Exception = ex;
            }
        }

#if NET45
        /// <summary>
        /// Sets the underling value after the query has been executed.
        /// </summary>
        /// <param name="dataContext">The data context to translate the results with.</param>
        /// <param name="reader">The <see cref="DbDataReader" /> to get the result from.</param>
        /// <param name="cancellationToken"></param>
        /// <returns></returns>
        Task IFutureQuery.SetResultAsync(ObjectContext dataContext, DbDataReader reader, CancellationToken cancellationToken = default(CancellationToken))
        {
            return SetResultAsync(dataContext, reader, cancellationToken);
        }

        /// <summary>
        /// Sets the underling value after the query has been executed.
        /// </summary>
        /// <param name="dataContext">The data context to translate the results with.</param>
        /// <param name="reader">The <see cref="DbDataReader" /> to get the result from.</param>
        /// <param name="cancellationToken"></param>
        /// <returns></returns>
        /// <exception cref="InvalidOperationException">The future query is not of type ObjectQuery.</exception>
        protected virtual async Task SetResultAsync(ObjectContext dataContext, DbDataReader reader, CancellationToken cancellationToken = default(CancellationToken))
        {
            _isLoaded = true;

            try
            {
                IFutureQuery futureQuery = this;
                var source = futureQuery.Query;

                var q = source as ObjectQuery;
                if (q == null)
                    throw new InvalidOperationException("The future query is not of type ObjectQuery.");

                // create execution plan
                dynamic queryProxy = new DynamicProxy(q);
                // ObjectQueryState
                dynamic queryState = queryProxy.QueryState;

                // ObjectQueryExecutionPlan
                dynamic executionPlan = queryState.GetExecutionPlan(null);

                // ShaperFactory
                dynamic shaperFactory = executionPlan.ResultShaperFactory;
                // Shaper<T>
                dynamic shaper = shaperFactory.Create(reader, dataContext, dataContext.MetadataWorkspace,
                    MergeOption.AppendOnly, false, true, false);

                var list = new List<T>();
                // Shaper<T> GetEnumerator method return IDbEnumerator<T>, which implements publicly accessible IDbAsyncEnumerator<T>
                IDbAsyncEnumerator<T> enumerator = shaper.GetEnumerator();
                while (await enumerator.MoveNextAsync(cancellationToken).ConfigureAwait(false))
                    list.Add(enumerator.Current);

                _result = list;

                // translate has issue with column names not matching
                //var resultSet = dataContext.Translate<T>(reader);
                //_result = resultSet.ToList();
            }
            catch (Exception ex)
            {
                Exception = ex;
            }
        }
#endif
    }
}