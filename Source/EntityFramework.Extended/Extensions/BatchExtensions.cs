using System;
using System.Data.Entity;
using System.Linq;
using System.Linq.Expressions;
using System.Data.Objects;
using EntityFramework.Batch;
using EntityFramework.Mapping;

namespace EntityFramework.Extensions
{
    /// <summary>
    /// An extensions class for batch queries.
    /// </summary>
    public static class BatchExtensions
    {
        /// <summary>
        /// The API was refactored to no longer need this extension method. Use query.Where(expression).Delete() syntax instead.
        /// </summary>
        /// <typeparam name="TEntity">The type of the entity.</typeparam>
        /// <param name="source">The source used to determine the table to delete from.</param>
        /// <param name="query">The IQueryable used to generate the where clause for the delete statement.</param>
        /// <returns>The number of row deleted.</returns>
        /// <remarks>
        /// When executing this method, the statement is immediately executed on the database provider
        /// and is not part of the change tracking system.  Also, changes will not be reflected on 
        /// any entities that have already been materialized in the current context.        
        /// </remarks>
        [Obsolete("The API was refactored to no longer need this extension method. Use query.Where(expression).Delete() syntax instead.")]
        public static int Delete<TEntity>(
           this IQueryable<TEntity> source,
           IQueryable<TEntity> query)
           where TEntity : class
        {
            return query.Delete();
        }

        /// <summary>
        /// Executes a delete statement using an expression to filter the rows to be deleted.
        /// </summary>
        /// <typeparam name="TEntity">The type of the entity.</typeparam>
        /// <param name="source">The source used to determine the table to delete from.</param>
        /// <param name="filterExpression">The filter expression used to generate the where clause for the delete statement.</param>
        /// <returns>The number of row deleted.</returns>
        /// <example>Delete all users with email domain @test.com.
        /// <code><![CDATA[
        /// var db = new TrackerContext();
        /// string emailDomain = "@test.com";
        /// int count = db.Users.Delete(u => u.Email.EndsWith(emailDomain));
        /// ]]></code>
        /// </example>
        /// <remarks>
        /// When executing this method, the statement is immediately executed on the database provider
        /// and is not part of the change tracking system.  Also, changes will not be reflected on 
        /// any entities that have already been materialized in the current context.        
        /// </remarks>
        public static int Delete<TEntity>(
            this IQueryable<TEntity> source,
            Expression<Func<TEntity, bool>> filterExpression)
            where TEntity : class
        {
            if (source == null)
                throw new ArgumentNullException("source");
            if (filterExpression == null)
                throw new ArgumentNullException("filterExpression");

            return source.Where(filterExpression).Delete();
        }

        /// <summary>
        /// Executes a delete statement using the query to filter the rows to be deleted.
        /// </summary>
        /// <typeparam name="TEntity">The type of the entity.</typeparam>
        /// <param name="source">The <see cref="IQueryable`1"/> used to generate the where clause for the delete statement.</param>
        /// <returns>The number of row deleted.</returns>
        /// <example>Delete all users with email domain @test.com.
        /// <code><![CDATA[
        /// var db = new TrackerContext();
        /// string emailDomain = "@test.com";
        /// int count = db.Users.Where(u => u.Email.EndsWith(emailDomain)).Delete();
        /// ]]></code>
        /// </example>
        /// <remarks>
        /// When executing this method, the statement is immediately executed on the database provider
        /// and is not part of the change tracking system.  Also, changes will not be reflected on 
        /// any entities that have already been materialized in the current context.        
        /// </remarks>
        public static int Delete<TEntity>(this IQueryable<TEntity> source) 
            where TEntity : class
        {
            ObjectQuery<TEntity> sourceQuery = source.ToObjectQuery();
            if (sourceQuery == null)
                throw new ArgumentException("The query must be of type ObjectQuery or DbQuery.", "source");

            ObjectContext objectContext = sourceQuery.Context;
            if (objectContext == null)
                throw new ArgumentException("The ObjectContext for the source query can not be null.", "source");

            EntityMap entityMap = sourceQuery.GetEntityMap<TEntity>();
            if (entityMap == null)
                throw new ArgumentException("Could not load the entity mapping information for the query ObjectSet.", "source");

            var runner = ResolveRunner();
            return runner.Delete(objectContext, entityMap, sourceQuery);
        }
        
        /// <summary>
        /// The API was refactored to no longer need this extension method. Use query.Where(expression).Update(updateExpression) syntax instead.
        /// </summary>
        /// <typeparam name="TEntity">The type of the entity.</typeparam>
        /// <param name="source">The source used to determine the table to update.</param>
        /// <param name="query">The query used to generate the where clause.</param>
        /// <param name="updateExpression">The MemberInitExpression used to indicate what is updated.</param>
        /// <returns>The number of row updated.</returns>
        /// <remarks>
        /// When executing this method, the statement is immediately executed on the database provider
        /// and is not part of the change tracking system.  Also, changes will not be reflected on 
        /// any entities that have already been materialized in the current context.        
        /// </remarks>
        [Obsolete("The API was refactored to no longer need this extension method. Use query.Where(expression).Update(updateExpression) syntax instead.")]
        public static int Update<TEntity>(
            this IQueryable<TEntity> source,
            IQueryable<TEntity> query,
            Expression<Func<TEntity, TEntity>> updateExpression)
            where TEntity : class
        {
            if (source == null)
                throw new ArgumentNullException("source");
            if (query == null)
                throw new ArgumentNullException("query");
            if (updateExpression == null)
                throw new ArgumentNullException("updateExpression");

            ObjectQuery<TEntity> sourceQuery = source.ToObjectQuery();
            if (sourceQuery == null)
                throw new ArgumentException("The query must be of type ObjectQuery or DbQuery.", "source");

            ObjectContext objectContext = sourceQuery.Context;
            if (objectContext == null)
                throw new ArgumentException("The ObjectContext for the source query can not be null.", "source");

            EntityMap entityMap = sourceQuery.GetEntityMap<TEntity>();
            if (entityMap == null)
                throw new ArgumentException("Could not load the entity mapping information for the source.", "source");

            ObjectQuery<TEntity> objectQuery = query.ToObjectQuery();
            if (objectQuery == null)
                throw new ArgumentException("The query must be of type ObjectQuery or DbQuery.", "query");

            var runner = ResolveRunner();
            return runner.Update(objectContext, entityMap, objectQuery, updateExpression);
        }

        /// <summary>
        /// Executes an update statement using an expression to filter the rows that are updated.
        /// </summary>
        /// <typeparam name="TEntity">The type of the entity.</typeparam>
        /// <param name="source">The source used to determine the table to update.</param>
        /// <param name="filterExpression">The filter expression used to generate the where clause.</param>
        /// <param name="updateExpression">The <see cref="MemberInitExpression"/> used to indicate what is updated.</param>
        /// <returns>The number of row updated.</returns>
        /// <example>Update all users in the test.com domain to be inactive.
        /// <code><![CDATA[
        /// var db = new TrackerContext();
        /// string emailDomain = "@test.com";
        /// int count = db.Users.Update(
        ///   u => u.Email.EndsWith(emailDomain),
        ///   u => new User { IsApproved = false, LastActivityDate = DateTime.Now });
        /// ]]></code>
        /// </example>
        /// <remarks>
        /// When executing this method, the statement is immediately executed on the database provider
        /// and is not part of the change tracking system.  Also, changes will not be reflected on 
        /// any entities that have already been materialized in the current context.        
        /// </remarks>
        public static int Update<TEntity>(
            this IQueryable<TEntity> source,
            Expression<Func<TEntity, bool>> filterExpression,
            Expression<Func<TEntity, TEntity>> updateExpression)
            where TEntity : class
        {
            if (source == null)
                throw new ArgumentNullException("source");
            if (filterExpression == null)
                throw new ArgumentNullException("filterExpression");

            return source.Where(filterExpression).Update(updateExpression);
        }

        /// <summary>
        /// Executes an update statement using the query to filter the rows to be updated.
        /// </summary>
        /// <typeparam name="TEntity">The type of the entity.</typeparam>
        /// <param name="source">The query used to generate the where clause.</param>
        /// <param name="updateExpression">The <see cref="MemberInitExpression"/> used to indicate what is updated.</param>
        /// <returns>The number of row updated.</returns>
        /// <example>Update all users in the test.com domain to be inactive.
        /// <code><![CDATA[
        /// var db = new TrackerContext();
        /// string emailDomain = "@test.com";
        /// int count = db.Users
        ///   .Where(u => u.Email.EndsWith(emailDomain))
        ///   .Update(u => new User { IsApproved = false, LastActivityDate = DateTime.Now });
        /// ]]></code>
        /// </example>
        /// <remarks>
        /// When executing this method, the statement is immediately executed on the database provider
        /// and is not part of the change tracking system.  Also, changes will not be reflected on 
        /// any entities that have already been materialized in the current context.        
        /// </remarks>
        public static int Update<TEntity>(
            this IQueryable<TEntity> source,
            Expression<Func<TEntity, TEntity>> updateExpression)
            where TEntity : class
        {
            if (source == null)
                throw new ArgumentNullException("source");
            if (updateExpression == null)
                throw new ArgumentNullException("updateExpression");

            ObjectQuery<TEntity> sourceQuery = source.ToObjectQuery();
            if (sourceQuery == null)
                throw new ArgumentException("The query must be of type ObjectQuery or DbQuery.", "source");

            ObjectContext objectContext = sourceQuery.Context;
            if (objectContext == null)
                throw new ArgumentException("The ObjectContext for the query can not be null.", "source");

            EntityMap entityMap = sourceQuery.GetEntityMap<TEntity>();
            if (entityMap == null)
                throw new ArgumentException("Could not load the entity mapping information for the source.", "source");
            
            var runner = ResolveRunner();
            return runner.Update(objectContext, entityMap, sourceQuery, updateExpression);
        }

        /// <summary>
        /// Executes a bulk insert statement, inserting a set of records.
        /// </summary>
        /// <typeparam name="TEntity">The type of the entity.</typeparam>
        /// <param name="destination">The <see cref="DbSet"/> to insert the records into.</param>
        /// <param name="records">Collection of <typeparamref name="TEntity"/> to be inserted</param>
        /// <returns>The number of row inserted.</returns>
        /// <remarks>
        /// When executing this method, the statement is immediately executed on the database provider
        /// and is not part of the change tracking system.
        /// </remarks>
        public static int BulkInsert<TEntity>(this DbSet<TEntity> destination, IEnumerable<TEntity> records) where TEntity : class
        {
            if (destination == null)
                throw new ArgumentNullException("source");
            if (records == null)
                throw new ArgumentNullException("records");

            ObjectQuery<TEntity> sourceQuery = destination.ToObjectQuery();
            if (sourceQuery == null)
                throw new ArgumentException("The query must be of type ObjectQuery or DbQuery.", "source");

            ObjectContext objectContext = sourceQuery.Context;
            if (objectContext == null)
                throw new ArgumentException("The ObjectContext for the source query can not be null.", "source");

            EntityMap entityMap = sourceQuery.GetEntityMap<TEntity>();
            if (entityMap == null)
                throw new ArgumentException("Could not load the entity mapping information for the source.", "source");

            var runner = ResolveRunner();
            return runner.BulkInsert(objectContext, entityMap, records);
        }

        /// <summary>
        /// Executes an insert from statement, inserting a set of records.
        /// </summary>
        /// <typeparam name="TSource">The type of the source entity.</typeparam>
        /// <typeparam name="TEntity">The type of the entity to be inserted.</typeparam>
        /// <param name="destination">The <see cref="DbSet"/> to insert the records into.</param>
        /// <param name="source">The query from which to get the <typeparamref name="TSource"/> entities.</param>
        /// <param name="mappingExpression">The insert expression mapping <typeparamref name="TSource"/> to <typeparamref name="TEntity"/></param>
        /// <returns>The number of row inserted.</returns>
        /// <remarks>
        /// When executing this method, the statement is immediately executed on the database provider
        /// and is not part of the change tracking system.
        /// </remarks>
        public static int InsertFrom<TSource, TEntity>(this DbSet<TEntity> destination,
            IQueryable<TSource> source, Expression<Func<TSource, TEntity>> mappingExpression)
            where TEntity : class
            where TSource : class
        {
            if (destination == null)
                throw new ArgumentNullException("destination");
            if (source == null)
                throw new ArgumentNullException("query");
            if (mappingExpression == null)
                throw new ArgumentNullException("insertExpression");

            ObjectQuery<TEntity> destinationQuery = destination.ToObjectQuery();
            if (destinationQuery == null)
                throw new ArgumentException("The query must be of type ObjectQuery or DbQuery.", "destination");

            ObjectContext destinationContext = destinationQuery.Context;
            if (destinationContext == null)
                throw new ArgumentException("The ObjectContext for the destination query can not be null.", "destination");

            EntityMap destinationEntityMap = destinationQuery.GetEntityMap<TEntity>();
            if (destinationEntityMap == null)
                throw new ArgumentException("Could not load the entity mapping information for the destination.", "destination");

            ObjectQuery<TSource> sourceQuery = source.ToObjectQuery();
            if (sourceQuery == null)
                throw new ArgumentException("The source must be of type ObjectQuery or DbQuery.", "source");

            var runner = ResolveRunner();
            return runner.InsertFrom(destinationContext, destinationEntityMap, sourceQuery, mappingExpression);
        }

        private static IBatchRunner ResolveRunner()
        {
            var provider = Locator.Current.Resolve<IBatchRunner>();
            if (provider == null)
                throw new InvalidOperationException("Could not resolve the IBatchRunner. Make sure IBatchRunner is registered in the Locator.Current container.");

            return provider;
        }
    }
}
