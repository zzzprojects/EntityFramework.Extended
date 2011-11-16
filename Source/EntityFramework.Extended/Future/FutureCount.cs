using System;
using System.Diagnostics;
using System.Linq;

namespace EntityFramework.Future
{
    /// <summary>
    /// Provides for defering the execution of a count query to a batch of queries.
    /// </summary>
    /// <example>The following is an example of how to use FutureCount to page a 
    /// list and get the total count in one call.
    /// <code><![CDATA[
    /// var db = new TrackerDataContext { Log = Console.Out };
    /// // base query
    /// var q = db.Task.ByPriority(Priority.Normal).OrderBy(t => t.CreatedDate);
    /// // get total count
    /// var q1 = q.FutureCount();
    /// // get first page
    /// var q2 = q.Skip(0).Take(10).Future();
    /// // triggers sql execute as a batch
    /// var tasks = q2.ToList();
    /// int total = q1.Value;
    /// ]]>
    /// </code>
    /// </example>
    [DebuggerDisplay("IsLoaded={IsLoaded}, Value={ValueForDebugDisplay}")]
    public class FutureCount : FutureValue<int>
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="FutureCount"/> class.
        /// </summary>
        /// <param name="query">The query source to use when materializing.</param>
        /// <param name="loadAction">The action to execute when the query is accessed.</param>
        internal FutureCount(IQueryable query, Action loadAction)
            : base(query, loadAction)
        { }

        /// <summary>
        /// Initializes a new instance of the <see cref="FutureCount"/> class.
        /// </summary>
        /// <param name="underlyingValue">The underlying value.</param>
        public FutureCount(int underlyingValue)
            : base(underlyingValue)
        { }
    }


}
