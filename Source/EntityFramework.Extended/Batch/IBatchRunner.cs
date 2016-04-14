using System;
using System.Collections.Generic;
using System.Data.Entity.Core.Objects;
using System.Linq.Expressions;
using System.Threading.Tasks;
using EntityFramework.Mapping;
using System.Data.Common;
using System.Linq;
using System.Data.Entity;

namespace EntityFramework.Batch
{
    /// <summary>
    /// An interface to abstract the batch execute from expressions.
    /// </summary>
    public interface IBatchRunner
    {
        /// <summary>
        /// NULL value for a parameter of <see cref="DbCommand"/>, it may be <code>null</code> or <see cref="DBNull"/>
        /// </summary>
        object DbNull { get; }

        /// <summary>
        /// To quote an SQL identifier so that it's safe to be included in an SQL statement
        /// <param name="identifier">An identifier.</param>
        /// <returns>The quoted identifier</returns>
        /// </summary>
        string Quote(string identifier);

        /// <summary>
        /// Create and runs a batch delete statement.
        /// </summary>
        /// <typeparam name="TEntity">The type of the entity.</typeparam>
        /// <param name="objectContext">The <see cref="ObjectContext"/> to get connection and metadata information from.</param>
        /// <param name="entityMap">The <see cref="EntityMap"/> for <typeparamref name="TEntity"/>.</param>
        /// <param name="query">The query to create the where clause from.</param>
        /// <returns>The number of rows deleted.</returns>
        int Delete<TEntity>(ObjectContext objectContext, EntityMap entityMap, ObjectQuery<TEntity> query)
            where TEntity : class;

#if NET45
        /// <summary>
        /// Create and runs a batch delete statement asynchronously.
        /// </summary>
        /// <typeparam name="TEntity">The type of the entity.</typeparam>
        /// <param name="objectContext">The <see cref="ObjectContext"/> to get connection and metadata information from.</param>
        /// <param name="entityMap">The <see cref="EntityMap"/> for <typeparamref name="TEntity"/>.</param>
        /// <param name="query">The query to create the where clause from.</param>
        /// <returns>The number of rows deleted.</returns>
        Task<int> DeleteAsync<TEntity>(ObjectContext objectContext, EntityMap entityMap, ObjectQuery<TEntity> query)
            where TEntity : class;
#endif
        /// <summary>
        /// Create and runs a batch update statement.
        /// </summary>
        /// <typeparam name="TEntity">The type of the entity.</typeparam>
        /// <param name="objectContext">The <see cref="ObjectContext"/> to get connection and metadata information from.</param>
        /// <param name="entityMap">The <see cref="EntityMap"/> for <typeparamref name="TEntity"/>.</param>
        /// <param name="query">The query to create the where clause from.</param>
        /// <param name="updateExpression">The update expression.</param>
        /// <returns>The number of rows updated.</returns>
        int Update<TEntity>(ObjectContext objectContext, EntityMap entityMap, ObjectQuery<TEntity> query, Expression<Func<TEntity, TEntity>> updateExpression)
            where TEntity : class;

#if NET45
        /// <summary>
        /// Create and runs a batch update statement asynchronously.
        /// </summary>
        /// <typeparam name="TEntity">The type of the entity.</typeparam>
        /// <param name="objectContext">The <see cref="ObjectContext"/> to get connection and metadata information from.</param>
        /// <param name="entityMap">The <see cref="EntityMap"/> for <typeparamref name="TEntity"/>.</param>
        /// <param name="query">The query to create the where clause from.</param>
        /// <param name="updateExpression">The update expression.</param>
        /// <returns>The number of rows updated.</returns>
        Task<int> UpdateAsync<TEntity>(ObjectContext objectContext, EntityMap entityMap, ObjectQuery<TEntity> query, Expression<Func<TEntity, TEntity>> updateExpression)
            where TEntity : class;
#endif

        /// <summary>
        /// Execute statement `<code>INSERT INTO [Table] (...) SELECT ...</code>`.
        /// </summary>
        /// <typeparam name="TModel">The type <paramref name="query"/> item.</typeparam>
        /// <param name="query">The query to create SELECT clause statement.</param>
        /// <param name="objectQuery">The query to create SELECT clause statement and it can also be used to get the information of db connection via
        ///     <code>objectQuery.Context</code> property.</param>
        /// <param name="entityMap">The <see cref="EntityMap"/> for entity type of the destination table (<see cref="IDbSet"/>).</param>
        /// <returns>
        /// The number of rows inserted.
        /// </returns>
        int Insert<TModel>(IQueryable<TModel> query, ObjectQuery<TModel> objectQuery, EntityMap entityMap) where TModel : class;

#if NET45
        /// <summary>
        /// Execute statement `<code>INSERT INTO [Table] (...) SELECT ...</code>`.
        /// </summary>
        /// <typeparam name="TModel">The type <paramref name="query"/> item.</typeparam>
        /// <param name="query">The query to create SELECT clause statement.</param>
        /// <param name="objectQuery">The query to create SELECT clause statement and it can also be used to get the information of db connection via
        ///     <code>objectQuery.Context</code> property.</param>
        /// <param name="entityMap">The <see cref="EntityMap"/> for entity type of the destination table (<see cref="IDbSet"/>).</param>
        /// <returns>
        /// The number of rows inserted.
        /// </returns>
        Task<int> InsertAsync<TModel>(IQueryable<TModel> query, ObjectQuery<TModel> objectQuery, EntityMap entityMap) where TModel : class;
#endif
    }
}
