using System;
using System.Collections.Generic;
using System.Data.Entity.Core.Objects;
using System.Data.Entity.Infrastructure;
using System.Linq;
using System.Linq.Expressions;
using EntityFramework.Reflection;

namespace EntityFramework.Extensions
{
    /// <summary>
    /// Extension methods for for ObjectQuery.
    /// </summary>
  public static class ObjectQueryExtensions
  {
      /// <summary>
      /// Convert the query into an ObjectQuery.
      /// </summary>
      /// <typeparam name="TEntity">The type of the entity.</typeparam>
      /// <param name="query">The query to convert.</param>
      /// <returns>The converted ObjectQuery; otherwise null if it can't be converted.</returns>
      public static ObjectQuery<TEntity> ToObjectQuery<TEntity>(this IQueryable<TEntity> query) where TEntity : class
      {
          var queryUnwrapper = Locator.Current.Resolve<IQueryUnwrapper>();
          if (queryUnwrapper != null)
              query = queryUnwrapper.Unwrap(query);

          // first try direct cast
          var objectQuery = query as ObjectQuery<TEntity>;
          if (objectQuery != null)
              return objectQuery;

          // next try case to DbQuery
          var dbQuery = query as DbQuery<TEntity>;
          if (dbQuery == null)
              return null;

          // access internal property InternalQuery
          dynamic dbQueryProxy = new DynamicProxy(dbQuery);
          dynamic internalQuery = dbQueryProxy.InternalQuery;
          if (internalQuery == null)
              return null;

          // access internal property ObjectQuery
          dynamic objectQueryProxy = internalQuery.ObjectQuery;
          if (objectQueryProxy == null)
              return null;

          // convert dynamic to static type
          objectQuery = objectQueryProxy;
          return objectQuery;
      }

      /// <summary>
      /// Convert the query into an ObjectQuery.
      /// </summary>
      /// <param name="query">The query to convert.</param>
      /// <returns>The converted ObjectQuery; otherwise null if it can't be converted.</returns>
      public static ObjectQuery ToObjectQuery(this IQueryable query)
      {
          var queryUnwrapper = Locator.Current.Resolve<IQueryUnwrapper>();
          if (queryUnwrapper != null)
              query = queryUnwrapper.Unwrap(query);

          // first try direct cast
          var objectQuery = query as ObjectQuery;
          if (objectQuery != null)
              return objectQuery;

          // next try case to DbQuery
          var dbQuery = query as DbQuery;
          if (dbQuery == null)
              return null;

          // access internal property InternalQuery
          dynamic dbQueryProxy = new DynamicProxy(dbQuery);
          dynamic internalQuery = dbQueryProxy.InternalQuery;
          if (internalQuery == null)
              return null;

          // access internal property ObjectQuery
          dynamic objectQueryProxy = internalQuery.ObjectQuery;
          if (objectQueryProxy == null)
              return null;

          // convert dynamic to static type
          objectQuery = objectQueryProxy;
          return objectQuery;
      }

      /// <summary>
      /// Creates an ObjectQuery from an expression.
      /// </summary>
      /// <param name="source">The source.</param>
      /// <param name="expression">The expression.</param>
      /// <param name="type">The type.</param>
      /// <returns>An ObjectQuery created from the expression.</returns>
      public static ObjectQuery CreateQuery(this IQueryable source, Expression expression, Type type)
      {
          // first convert to object query to get the correct provider
          ObjectQuery sourceQuery = source.ToObjectQuery();
          if (sourceQuery == null)
              return null;

          IQueryProvider provider = ((IQueryable)sourceQuery).Provider;

          // create query from expression using internal ObjectQueryProvider
          dynamic providerProxy = new DynamicProxy(provider);
          IQueryable expressionQuery = providerProxy.CreateQuery(expression, type);
          if (expressionQuery == null)
              return null;

          // convert back to ObjectQuery
          ObjectQuery resultQuery = expressionQuery.ToObjectQuery();
          return resultQuery;
      }

      /// <summary>
      /// Gets the ObjectContext for the specifed query.
      /// </summary>
      /// <typeparam name="TEntity">The type of the entity.</typeparam>
      /// <param name="query">The query.</param>
      /// <returns>The ObjectContext for the query.</returns>
      public static ObjectContext GetContext<TEntity>(this IQueryable<TEntity> query) where TEntity : class
      {
          var objectQuery = query.ToObjectQuery();
          if (objectQuery != null)
              return objectQuery.Context;

          return null;
      }

  }
}
