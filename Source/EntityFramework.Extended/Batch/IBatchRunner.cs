using System;
using System.Collections.Generic;
using System.Data.Objects;
using System.Linq.Expressions;
using EntityFramework.Mapping;

namespace EntityFramework.Batch
{
    /// <summary>
    /// An interface to abstract the batch execute from expressions.
    /// </summary>
    public interface IBatchRunner
    {
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

        /// <summary>
        /// Create and runs a batch insert statement.
        /// </summary>
        /// <typeparam name="TEntity">The type of the entity to be inserted</typeparam>
        /// <param name="objectContext">The <see cref="ObjectContext"/> to get connection and metadata information from.</param>
        /// <param name="entityMap">The <see cref="EntityMap"/> for <typeparamref name="TEntity"/>.</param>
        /// <param name="records">Collection of <typeparamref name="TEntity"/> to be inserted</param>
        /// <returns>The number of rows inserted.</returns>
        int BulkInsert<TEntity>(ObjectContext objectContext, EntityMap entityMap, IEnumerable<TEntity> records)
            where TEntity : class;

        /// <summary>
        /// Create and runs a batch insert from statement.
        /// </summary>
        /// <typeparam name="TSource">The type of the source entity.</typeparam>
        /// <typeparam name="TEntity">The type of the entity to be inserted.</typeparam>
        /// <param name="destinationContext">The <see cref="ObjectContext"/> to get connection and metadata information from.</param>
        /// <param name="destinationEntityMap">The <see cref="EntityMap"/> for <typeparamref name="TEntity"/>.</param>
        /// <param name="sourceQuery">The query from which to get the <typeparamref name="TSource"/> entities.</param>
        /// <param name="mappingExpression">The insert expression mapping <typeparamref name="TSource"/> to <typeparamref name="TEntity"/></param>
        /// <returns>The number of rows inserted.</returns>
        int InsertFrom<TSource, TEntity>(ObjectContext destinationContext, EntityMap destinationEntityMap, ObjectQuery<TSource> sourceQuery, Expression<Func<TSource, TEntity>> mappingExpression)
            where TEntity : class
            where TSource : class;
    }
}
