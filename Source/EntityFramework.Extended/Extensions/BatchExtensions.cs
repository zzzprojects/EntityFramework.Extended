using System;
using System.Data.Entity;
using System.Data.Entity.Core.Objects;
using System.Linq;
using System.Linq.Expressions;
using System.Threading.Tasks;
using EntityFramework.Batch;
using EntityFramework.Mapping;
using System.Collections.Generic;

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

        private static Tuple<ObjectContext, EntityMap> GetQueryObjects<TEntity>(IDbSet<TEntity> dbSet) where TEntity : class
        {
            if (dbSet == null)
                throw new ArgumentNullException("dbSet");

            ObjectQuery<TEntity> destQuery = dbSet.ToObjectQuery();
            if (destQuery == null)
                throw new ArgumentException("The DbSet must be of type ObjectQuery or DbQuery.", "dbSet");

            ObjectContext destContext = destQuery.Context;
            if (destContext == null)
                throw new ArgumentException("The ObjectContext for the DbSet can not be null.", "dbSet");

            EntityMap entityMap = destQuery.GetEntityMap<TEntity>();
            if (entityMap == null)
                throw new ArgumentException("Could not load the entity mapping information for the destination.", "dbSet");
            return Tuple.Create(destContext, entityMap);
        }

        private static Tuple<ObjectQuery<TModel>, EntityMap> CheckDbQueryParams<TEntity, TModel>(IDbSet<TEntity> destination,
            IQueryable<TModel> source)
            where TEntity : class
            where TModel : class
        {
            EntityMap entityMap = GetQueryObjects(destination).Item2;

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
        public static int Update<TEntity, TModel>(
            this IDbSet<TEntity> destination,
            IQueryable<TModel> source)
            where TEntity : class
            where TModel : class
        {
            var p = CheckDbQueryParams(destination, source);
            return ResolveRunner().Update(source, p.Item1, p.Item2);
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
        public static Task<int> UpdateAsync<TEntity, TModel>(
            this IDbSet<TEntity> destination,
            IQueryable<TModel> source)
            where TEntity : class
            where TModel : class
        {
            var p = CheckDbQueryParams(destination, source);
            return ResolveRunner().UpdateAsync(source, p.Item1, p.Item2);
        }
#endif

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
            var p = CheckDbQueryParams(destination, source);
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
            var p = CheckDbQueryParams(destination, source);
            return ResolveRunner().InsertAsync(source, p.Item1, p.Item2);
        }
#endif

        /// <summary>
        /// Inserts a lof of rows into a database table. It must be much faster than executing `<code>DbSet.AddRange</code>` or
        /// repetitive `<code>DbSet.Add</code>` method and then executing '<code>DbContext.SaveChanges</code>' method.
        /// </summary>
        /// <typeparam name="TEntity">The type of objects representing rows to be inserted into db table.</typeparam>
        /// <param name="dbSet">The <code>IDbSet</code> object representing the table to which the rows will be inserted.</param>
        /// <param name="entities">The entity objects reprsenting the rows that will be inserted.</param>
        /// <param name="batchSize">Number of rows in each batch. At the end of each batch, the rows in the batch are sent to the server. Zero means there
        /// will be a single batch</param>
        /// <param name="timeout">Number of seconds for the operation to complete before it times out. Zero means no limit.</param>
        /// <returns>
        /// The number of rows inserted.
        /// </returns>
        public static int Insert<TEntity>(this IDbSet<TEntity> dbSet, IEnumerable<TEntity> entities, int batchSize = 1000, int timeout = 0)
            where TEntity : class
        {
            var objects = GetQueryObjects(dbSet);
            return ResolveRunner().Insert(objects.Item1, entities, objects.Item2, batchSize, timeout);
        }

#if NET45
        /// <summary>
        /// Inserts a lof of rows into a database table asynchronously. It must be much faster than executing `<code>DbSet.AddRange</code>` or
        /// repetitive `<code>DbSet.Add</code>` method and then executing '<code>DbContext.SaveChanges</code>' method.
        /// </summary>
        /// <typeparam name="TEntity">The type of objects representing rows to be inserted into db table.</typeparam>
        /// <param name="dbSet">The <code>IDbSet</code> object representing the table to which the rows will be inserted.</param>
        /// <param name="entities">The entity objects reprsenting the rows that will be inserted.</param>
        /// <param name="batchSize">Number of rows in each batch. At the end of each batch, the rows in the batch are sent to the server. Zero means there
        /// will be a single batch</param>
        /// <param name="timeout">Number of seconds for the operation to complete before it times out. Zero means no limit.</param>
        /// <returns>
        /// The number of rows inserted.
        /// </returns>
        public static async Task<int> InsertAsync<TEntity>(this IDbSet<TEntity> dbSet, IEnumerable<TEntity> entities, int batchSize = 1000, int timeout = 0)
            where TEntity : class
        {
            var objects = GetQueryObjects(dbSet);
            return await ResolveRunner().InsertAsync(objects.Item1, entities, objects.Item2, batchSize, timeout);
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
