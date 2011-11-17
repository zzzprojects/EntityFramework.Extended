using System;
using System.Collections.Generic;
using System.Data.Common;
using System.Data.Entity.Infrastructure;
using System.Data.Objects;
using System.Linq;
using System.Linq.Expressions;
using System.Text;
using EntityFramework.Reflection;

namespace EntityFramework.Extensions
{
  public static class ObjectQueryExtensions
  {
      public static ObjectQuery<TEntity> ToObjectQuery<TEntity>(this IQueryable<TEntity> query) where TEntity : class
      {
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

      public static ObjectQuery ToObjectQuery(this IQueryable query)
      {
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

      public static ObjectQuery CreateQuery(this IQueryable source, Expression expression, Type type)
      {
          // first convet to object query to get the correct provider
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

      public static ObjectContext GetContext<TEntity>(this IQueryable<TEntity> query) where TEntity : class
      {
          var objectQuery = query.ToObjectQuery();
          if (objectQuery != null)
              return objectQuery.Context;

          return null;
      }

  }
}
