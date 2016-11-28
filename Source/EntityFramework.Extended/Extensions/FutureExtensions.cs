using System;
using System.Data.Entity.Core.Objects;
using System.Linq;
using System.Linq.Expressions;
using EntityFramework.Future;
using EntityFramework.Reflection;

namespace EntityFramework.Extensions
{
    /// <summary>
    /// Extension methods for future queries.
    /// </summary>
    /// <seealso cref="T:EntityFramework.Future.FutureQuery`1"/>
    /// <seealso cref="T:EntityFramework.Future.FutureValue`1"/>
    /// <seealso cref="T:EntityFramework.Future.FutureCount"/>
    public static class FutureExtensions
    {
        /// <summary>
        /// Provides for defering the execution of the <paramref name="source" /> query to a batch of future queries.
        /// </summary>
        /// <typeparam name="TEntity">The type of the elements of <paramref name="source" />.</typeparam>
        /// <param name="source">An <see cref="T:System.Linq.IQueryable`1" /> to add to the batch of future queries.</param>
        /// <returns>An <see cref="T:System.Collections.Generic.IEnumerable`1" /> that contains elements from the input sequence.</returns>
        /// <seealso cref="T:EntityFramework.Future.FutureQuery`1"/>
        public static FutureQuery<TEntity> Future<TEntity>(this IQueryable<TEntity> source)
          where TEntity : class
        {
            if (source == null)
                throw new ArgumentNullException("source");

            ObjectQuery<TEntity> sourceQuery = source.ToObjectQuery();
            if (sourceQuery == null)
                throw new ArgumentException("The source query must be of type ObjectQuery or DbQuery.", "source");

            var futureContext = GetFutureContext(sourceQuery);
            var future = new FutureQuery<TEntity>(sourceQuery, futureContext);
            futureContext.AddQuery(future);

            return future;
        }

        /// <summary>
        /// Provides for defering the execution of the <paramref name="source" /> query to a batch of future queries.
        /// </summary>
        /// <typeparam name="TEntity">The type of the elements of <paramref name="source" />.</typeparam>
        /// <param name="source">An <see cref="T:System.Linq.IQueryable`1" /> to add to the batch of future queries.</param>
        /// <returns>An instance of <see cref="FutureCount"/> that contains the result of the query.</returns>
        /// <seealso cref="T:EntityFramework.Future.FutureCount"/>
        public static FutureCount FutureCount<TEntity>(this IQueryable<TEntity> source)
            where TEntity : class
        {
            if (source == null)
                return new FutureCount(0);

            ObjectQuery<TEntity> sourceQuery = source.ToObjectQuery();
            if (sourceQuery == null)
                throw new ArgumentException("The source query must be of type ObjectQuery or DbQuery.", "source");

            // create count expression
            var expression = Expression.Call(
              typeof(Queryable),
              "Count",
              new[] { source.ElementType },
              source.Expression);

            // create query from expression using internal ObjectQueryProvider
            var countQuery = sourceQuery.CreateQuery(expression, typeof(int)) as ObjectQuery<int>;
            if (countQuery == null)
                throw new ArgumentException("The source query must be of type ObjectQuery or DbQuery.", "source");

            var futureContext = GetFutureContext(sourceQuery);
            var future = new FutureCount(countQuery, futureContext);
            futureContext.AddQuery(future);
            return future;
        }

        /// <summary>
        /// Provides for defering the execution of the <paramref name="source" /> query to a batch of future queries.
        /// </summary>
        /// <typeparam name="TEntity">The type of the elements of <paramref name="source" />.</typeparam>
        /// <typeparam name="TResult">The type of the result value wrapped in a <see cref="T:EntityFramework.Future.FutureValue`1"/>.</typeparam>
        /// <param name="source">An <see cref="T:System.Linq.IQueryable`1" /> to add to the batch of future queries.</param>
        /// <param name="selector">A lambda expression with one of the Min, Max, Count, Sum, Average aggregate functions</param>
        /// <returns>An instance of <see cref="T:EntityFramework.Future.FutureValue`1" /> that contains the result of the query</returns>
        public static FutureValue<TResult> FutureValue<TEntity, TResult>(this IQueryable<TEntity> source, Expression<Func<IQueryable<TEntity>, TResult>> selector)
            where TEntity : class
        {
            if (source == null)
                return new FutureValue<TResult>(default(TResult));

            var sourceQuery = source.ToObjectQuery();
            if (sourceQuery == null)
                throw new ArgumentException("The source query must be of type ObjectQuery or DbQuery.", "source");

            var methodExpr = selector.Body as MethodCallExpression;
            if (methodExpr == null || methodExpr.Arguments.Count == 0)
                throw new ArgumentException("The body of lambda expression must be a Count, Min, Max, Sum or Average method call", "selector");

            var arguments = new Expression[methodExpr.Arguments.Count];
            // the first argument is our source query
            arguments[0] = source.Expression; 
            // copy the rest of the arguments from method call expression
            for (int i = 1; i < arguments.Length; i++)
            {
                arguments[i] = methodExpr.Arguments[i];
            }

            var expression = Expression.Call(null, methodExpr.Method, arguments);
            var valueQuery = sourceQuery.CreateQuery(expression, typeof(TResult)) as ObjectQuery<TResult>;
            if (valueQuery == null)
                throw new ArgumentException("The source query must be of type ObjectQuery or DbQuery.", "source");

            var futureContext = GetFutureContext(sourceQuery);
            var future = new FutureValue<TResult>(valueQuery, futureContext);
            futureContext.AddQuery(future);
            return future;
        }

        /// <summary>
        /// Provides for defering the execution of the <paramref name="source" /> query to a batch of future queries.
        /// </summary>
        /// <typeparam name="TEntity">The type of the elements of <paramref name="source" />.</typeparam>
        /// <param name="source">An <see cref="T:System.Linq.IQueryable`1" /> to add to the batch of future queries.</param>
        /// <returns>An instance of <see cref="T:EntityFramework.Future.FutureValue`1"/> that contains the result of the query.</returns>
        /// <seealso cref="T:EntityFramework.Future.FutureValue`1"/>
        public static FutureValue<TEntity> FutureFirstOrDefault<TEntity>(this IQueryable<TEntity> source)
          where TEntity : class
        {
            if (source == null)
                return new FutureValue<TEntity>(default(TEntity));

            ObjectQuery sourceQuery = source.ToObjectQuery();
            if (sourceQuery == null)
                throw new ArgumentException("The source query must be of type ObjectQuery or DbQuery.", "source");

            // make sure to only get the first value
            IQueryable<TEntity> firstQuery = source.Take(1);

            ObjectQuery<TEntity> objectQuery = firstQuery.ToObjectQuery();
            if (objectQuery == null)
                throw new ArgumentException("The source query must be of type ObjectQuery or DbQuery.", "source");

            var futureContext = GetFutureContext(sourceQuery);
            var future = new FutureValue<TEntity>(objectQuery, futureContext);
            futureContext.AddQuery(future);
            return future;
        }

        private static IFutureContext GetFutureContext(ObjectQuery objectQuery)
        {
            // first try getting IFutureContext directly off ObjectContext
            var objectContext = objectQuery.Context as IFutureContext;
            if (objectContext != null)
                return objectContext;

            // next use FutureStore
            var futureContext = FutureStore.Default.GetOrCreate(objectQuery);
            return futureContext;
        }
    }
}
