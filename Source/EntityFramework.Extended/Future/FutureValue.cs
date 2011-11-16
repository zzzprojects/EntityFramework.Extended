using System;
using System.Diagnostics;
using System.Linq;

namespace EntityFramework.Future
{
    /// <summary>
    /// Provides for defering the execution of a query to a batch of queries.
    /// </summary>
    /// <typeparam name="T">The type for the future query.</typeparam>
    /// <example>The following is an example of how to use FutureValue.
    /// <code><![CDATA[
    /// var db = new TrackerDataContext { Log = Console.Out };
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
        /// Initializes a new instance of the <see cref="T:EntityFramework.Future.FutureValue`1"/> class.
        /// </summary>
        /// <param name="query">The query source to use when materializing.</param>
        /// <param name="loadAction">The action to execute when the query is accessed.</param>
        internal FutureValue(IQueryable query, Action loadAction)
            : base(query, loadAction)
        { }

        /// <summary>
        /// Initializes a new instance of the <see cref="T:EntityFramework.Future.FutureValue`1"/> class.
        /// </summary>
        /// <param name="underlyingValue">The underlying value.</param>
        public FutureValue(T underlyingValue)
            : base(null, null)
        {
            UnderlingValue = underlyingValue;
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
                    UnderlingValue = result.FirstOrDefault();
                }

                if (Exception != null)
                    throw new FutureException("An error occurred executing the future query.", Exception);

                return UnderlingValue;
            }
            set
            {
                UnderlingValue = value;
                _hasValue = true;
            }
        }

        /// <summary>
        /// Performs an implicit conversion from <see cref="T:EntityFramework.Future.FutureValue`1"/> to <see cref="T"/>.
        /// </summary>
        /// <param name="futureValue">The future value.</param>
        /// <returns>The result of forcing this lazy value.</returns>
        public static implicit operator T(FutureValue<T> futureValue)
        {
            return futureValue.Value;
        }

        /// <summary>
        /// Gets the underling value. This property will not trigger the loading of the future query.
        /// </summary>
        /// <value>The underling value.</value>
        internal T UnderlingValue { get; private set; }
    }
}
