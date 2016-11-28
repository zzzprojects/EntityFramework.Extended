using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace EntityFramework.Future
{
    /// <summary>
    /// Provides for defering the execution of a query to a batch of queries.
    /// </summary>
    /// <typeparam name="T">The type for the future query.</typeparam>
    /// <example>The following is an example of how to use FutureValue.
    /// <code><![CDATA[
    /// var db = new TrackeContext;
    /// // build up queries
    /// var q1 = db.User.ByEmailAddress("one@test.com").FutureValue();
    /// var q2 = db.Task.Where(t => t.Summary == "Test").Future();
    /// // this triggers the loading of all the future queries
    /// User user = q1.Value;
    /// var tasks = q2.ToList();
    /// ]]>
    /// </code>
    /// </example>
    [DebuggerDisplay("IsLoaded={IsLoaded}, Value={UnderlingValue}")]
    public class FutureValue<T> : FutureQueryBase<T>
    {
        private bool _hasValue;

        /// <summary>
        /// Initializes a new instance of the <see cref="T:EntityFramework.Future.FutureValue`1" /> class.
        /// </summary>
        /// <param name="query">The query source to use when materializing.</param>
        /// <param name="futureContext">The future context.</param>
        internal FutureValue(IQueryable<T> query, IFutureContext futureContext)
            : base(query, futureContext)
        { }

        /// <summary>
        /// Initializes a new instance of the <see cref="T:EntityFramework.Future.FutureValue`1"/> class.
        /// </summary>
        /// <param name="underlyingValue">The underlying value.</param>
        public FutureValue(T underlyingValue)
            : base(null, null)
        {
            UnderlyingValue = underlyingValue;
            _hasValue = true;
        }

        /// <summary>
        /// Gets or sets the value assigned to or loaded by the query.
        /// </summary>
        /// <returns>
        /// The value of this deferred property.
        /// </returns>
        [DebuggerBrowsable(DebuggerBrowsableState.Never)]
        public T Value
        {
            get
            {
                if (!_hasValue)
                {
                    _hasValue = true;

                    var result = GetResult() ?? Enumerable.Empty<T>();
                    UnderlyingValue = result.FirstOrDefault();
                }

                if (Exception != null)
                    throw new FutureException("An error occurred executing the future query.", Exception);

                return UnderlyingValue;
            }
            set
            {
                UnderlyingValue = value;
                _hasValue = true;
            }
        }

#if NET45
        /// <summary>
        /// Gets the value asynchronous.
        /// </summary>
        /// <param name="cancellationToken">The cancellation token.</param>
        /// <returns></returns>
        /// <exception cref="FutureException">An error occurred executing the future query.</exception>
        public async Task<T> GetValueAsync(CancellationToken cancellationToken = default(CancellationToken))
            {
            if (!_hasValue)
                {
                _hasValue = true;
                var resultingList = await GetResultAsync(cancellationToken).ConfigureAwait(false);
                UnderlyingValue = resultingList != null ? resultingList.FirstOrDefault() : default(T);
                }

            if (Exception != null)
                throw new FutureException("An error occurred executing the future query.", Exception);

            return UnderlyingValue;
            }
#endif

        /// <summary>
        /// Performs an implicit conversion from <see cref="T:EntityFramework.Future.FutureValue`1" /> to T.
        /// </summary>
        /// <param name="futureValue">The future value.</param>
        /// <returns>
        /// The result of forcing this lazy value.
        /// </returns>
        public static implicit operator T(FutureValue<T> futureValue)
        {
            return futureValue.Value;
        }

        /// <summary>
        /// Gets the underling value. This property will not trigger the loading of the future query.
        /// </summary>
        /// <value>The underling value.</value>
        internal T UnderlyingValue { get; private set; }
    }
}
