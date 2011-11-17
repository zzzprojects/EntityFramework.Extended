using System;
using System.Data.Objects;
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
            var future = new FutureQuery<TEntity>(sourceQuery, futureContext.ExecuteFutureQueries);
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

            ObjectQuery sourceQuery = source.ToObjectQuery();
            if (sourceQuery == null)
                throw new ArgumentException("The source query must be of type ObjectQuery or DbQuery.", "source");

            // create count expression
            var expression = Expression.Call(
              typeof(Queryable),
              "Count",
              new[] { source.ElementType },
              source.Expression);

            // create query from expression using internal ObjectQueryProvider
            ObjectQuery countQuery = sourceQuery.CreateQuery(expression, typeof(int));
            if (countQuery == null)
                throw new ArgumentException("The source query must be of type ObjectQuery or DbQuery.", "source");

            var futureContext = GetFutureContext(sourceQuery);
            var future = new FutureCount(countQuery, futureContext.ExecuteFutureQueries);
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
            var future = new FutureValue<TEntity>(objectQuery, futureContext.ExecuteFutureQueries);
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
