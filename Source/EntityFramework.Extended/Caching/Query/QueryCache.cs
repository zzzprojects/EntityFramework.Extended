using System;
using System.Linq;
using System.Linq.Expressions;

namespace EntityFramework.Caching
{
    /// <summary>
    /// Extension methods for query cache.
    /// </summary>
    /// <remarks>
    /// Copyright (c) 2010 Pete Montgomery.
    /// http://petemontgomery.wordpress.com
    /// Licenced under GNU LGPL v3.
    /// http://www.gnu.org/licenses/lgpl.html
    /// </remarks>
    public static class QueryCache
    {
        /// <summary>
        /// Gets a cache key for the specified <see cref="IQueryable"/>.
        /// </summary>
        /// <param name="query">The query to get the key from.</param>
        /// <returns>A unique key for the specified <see cref="IQueryable"/></returns>
        public static string GetCacheKey(this IQueryable query)
        {
            var expression = query.Expression;

            // locally evaluate as much of the query as possible
            expression = Evaluator.PartialEval(expression, QueryCache.CanBeEvaluatedLocally);

            // support local collections
            expression = LocalCollectionExpander.Rewrite(expression);

            // use the string representation of the expression for the cache key
            string key = expression.ToString();

            // the key is potentially very long, so use an md5 fingerprint
            // (fine if the query result data isn't critically sensitive)
            key = key.ToMd5Fingerprint();

            return key;
        }

        static Func<Expression, bool> CanBeEvaluatedLocally
        {
            get
            {
                return expression =>
                {
                    // don't evaluate parameters
                    if (expression.NodeType == ExpressionType.Parameter)
                        return false;

                    // can't evaluate queries
                    if (typeof(IQueryable).IsAssignableFrom(expression.Type))
                        return false;

                    return true;
                };
            }
        }
    }
}
