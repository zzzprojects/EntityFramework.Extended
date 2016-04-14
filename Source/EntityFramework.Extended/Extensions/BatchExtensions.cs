using System;
using System.Data.Entity;
using System.Data.Entity.Core.Objects;
using System.Linq;
using System.Linq.Expressions;
using System.Threading.Tasks;
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
        [Obsolete("The API was refactored to no longer need this extension method. Use query.Where(expression).Delete() syntax instead.")]
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
        /// <param name="source">The <see cref="T:IQueryable`1"/> used to generate the where clause for the delete statement.</param>
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

#if NET45
        /// <summary>
        /// Executes a delete statement asynchronously using an expression to filter the rows to be deleted.
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
        [Obsolete("The API was refactored to no longer need this extension method. Use query.Where(expression).DeleteAsync() syntax instead.")]
        public static Task<int> DeleteAsync<TEntity>(
            this IQueryable<TEntity> source,
            Expression<Func<TEntity, bool>> filterExpression)
            where TEntity : class
        {
            if (source == null)
                throw new ArgumentNullException("source");
            if (filterExpression == null)
                throw new ArgumentNullException("filterExpression");

            return source.Where(filterExpression).DeleteAsync();
        }

        /// <summary>
        /// Executes a delete statement asynchronously using the query to filter the rows to be deleted.
        /// </summary>
        /// <typeparam name="TEntity">The type of the entity.</typeparam>
        /// <param name="source">The <see cref="T:IQueryable`1"/> used to generate the where clause for the delete statement.</param>
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
        public static Task<int> DeleteAsync<TEntity>(this IQueryable<TEntity> source)
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
            return runner.DeleteAsync(objectContext, entityMap, sourceQuery);
        }

#endif

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
        [Obsolete("The API was refactored to no longer need this extension method. Use query.Where(expression).Update(updateExpression) syntax instead.")]
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

#if NET45
        /// <summary>
        /// Executes an update statement asynchronously using an expression to filter the rows that are updated.
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
        [Obsolete("The API was refactored to no longer need this extension method. Use query.Where(expression).UpdateAsync(updateExpression) syntax instead.")]
        public static Task<int> UpdateAsync<TEntity>(
            this IQueryable<TEntity> source,
            Expression<Func<TEntity, bool>> filterExpression,
            Expression<Func<TEntity, TEntity>> updateExpression)
            where TEntity : class
        {
            if (source == null)
                throw new ArgumentNullException("source");
            if (filterExpression == null)
                throw new ArgumentNullException("filterExpression");

            return source.Where(filterExpression).UpdateAsync(updateExpression);
        }

        /// <summary>
        /// Executes an update statement asynchronously using the query to filter the rows to be updated.
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
        public static Task<int> UpdateAsync<TEntity>(
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
            return runner.UpdateAsync(objectContext, entityMap, sourceQuery, updateExpression);
        }
#endif

        private static Tuple<ObjectQuery<TModel>, EntityMap> CheckInsertParams<TEntity, TModel>(IDbSet<TEntity> destination,
            IQueryable<TModel> source)
            where TEntity : class
            where TModel : class
        {
            if (destination == null)
                throw new ArgumentNullException("destination");
            if (source == null)
                throw new ArgumentNullException("source");

            ObjectQuery<TEntity> destQuery = destination.ToObjectQuery();
            if (destQuery == null)
                throw new ArgumentException("The DbSet must be of type ObjectQuery or DbQuery.", "destination");

            //ObjectContext destContext = destQuery.Context;
            //if (destContext == null)
            //    throw new ArgumentException("The ObjectContext for the DbSet can not be null.", "destination");

            EntityMap entityMap = destQuery.GetEntityMap<TEntity>();
            if (entityMap == null)
                throw new ArgumentException("Could not load the entity mapping information for the destination.", "destination");

            ObjectQuery<TModel> sourceQuery = source.ToObjectQuery();
            if (sourceQuery == null)
                throw new ArgumentException("The query must be of type ObjectQuery or DbQuery.", "source");

            ObjectContext sourceContext = sourceQuery.Context;
            if (sourceContext == null)
                throw new ArgumentException("The ObjectContext for the query can not be null.", "source");

            return Tuple.Create(sourceQuery, entityMap);
        }

        /// <summary>
        /// Executes a statement `<code>INSERT INTO [Table] (...) SELECT ...</code>`.
        /// </summary>
        /// <typeparam name="TEntity">The type of the entity representing a record in database table.</typeparam>
        /// <typeparam name="TModel">The type of the <paramref name="source"/> query item.</typeparam>
        /// <param name="destination">The target table where the new data will be inserted into.</param>
        /// <param name="source">The query which retrieves the new data that will be inserted into the target table.</param>
        /// <returns>The number of row inserted.</returns>
        /// <example>Copy all items whose product id "K9-RT-02" from table item into item_2.
        /// <code><![CDATA[
        /// var db = new TrackerContext();
        /// //var query = from item in db.items where item.ProductId == "K9-RT-02" select item; //Using MySQL provider, the parameter won't be caught if the parameter is constant
        /// string productId = "K9-RT-02";
        /// var query = from item in db.items where item.ProductId == productId select item;
        /// db.item_2.Insert(query);
        /// ]]></code>
        /// </example>
        /// <remarks>
        /// When executing this method, the statement is immediately executed on the database provider
        /// and is not part of the change tracking system.  Also, changes will not be reflected on 
        /// any entities that have already been materialized in the current context.        
        /// </remarks>
        public static int Insert<TEntity, TModel>(
            this IDbSet<TEntity> destination,
            IQueryable<TModel> source)
            where TEntity : class
            where TModel : class
        {
            var p = CheckInsertParams(destination, source);
            return ResolveRunner().Insert(source, p.Item1, p.Item2);
        }

#if NET45
        /// <summary>
        /// Executes a statement `<code>INSERT INTO [Table] (...) SELECT ...</code>` asynchronously.
        /// </summary>
        /// <typeparam name="TEntity">The type of the entity representing a record in database table.</typeparam>
        /// <typeparam name="TModel">The type of the <paramref name="source"/> query item.</typeparam>
        /// <param name="destination">The target table where the new data will be inserted into.</param>
        /// <param name="source">The query which retrieves the new data that will be inserted into the target table.</param>
        /// <returns>The number of row inserted.</returns>
        /// <example>Copy all items whose product id "K9-RT-02" from table item into item_2.
        /// <code><![CDATA[
        /// var db = new TrackerContext();
        /// //var query = from item in db.items where item.ProductId == "K9-RT-02" select item; //Using MySQL provider, the parameter won't be caught if the parameter is constant
        /// string productId = "K9-RT-02";
        /// var query = from item in db.items where item.ProductId == productId select item;
        /// db.item_2.InsertAsync(query);
        /// ]]></code>
        /// </example>
        /// <remarks>
        /// When executing this method, the statement is immediately executed on the database provider
        /// and is not part of the change tracking system.  Also, changes will not be reflected on 
        /// any entities that have already been materialized in the current context.        
        /// </remarks>
        public static Task<int> InsertAsync<TEntity, TModel>(
            this IDbSet<TEntity> destination,
            IQueryable<TModel> source)
            where TEntity : class
            where TModel : class
        {
            var p = CheckInsertParams(destination, source);
            return ResolveRunner().InsertAsync(source, p.Item1, p.Item2);
        }
#endif

        internal static IBatchRunner ResolveRunner()
        {
            var provider = Locator.Current.Resolve<IBatchRunner>();
            if (provider == null)
                throw new InvalidOperationException("Could not resolve the IBatchRunner. Make sure IBatchRunner is registered in the Locator.Current container.");

            return provider;
        }
    }
}
