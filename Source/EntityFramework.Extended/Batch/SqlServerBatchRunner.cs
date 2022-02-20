using System;
using System.Data;
using System.Data.Entity;
using System.Data.Common;
using System.Data.Entity.Core.EntityClient;
using System.Data.Entity.Core.Objects;
using System.Linq;
using System.Linq.Dynamic;
using System.Linq.Expressions;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using EntityFramework.Extensions;
using EntityFramework.Mapping;
using EntityFramework.Reflection;
using System.Collections.Generic;
using System.Data.SqlClient;

namespace EntityFramework.Batch
{
    /// <summary>
    /// A batch execution runner for SQL Server.
    /// </summary>
    public class SqlServerBatchRunner : IBatchRunner
    {
        /// <summary>
        /// NULL value for a parameter of <see cref="DbCommand"/>.
        /// </summary>
        public object DbNull { get { return DBNull.Value; } }

        /// <summary>
        /// To quote an SQL identifier so that it's safe to be included in an SQL statement
        /// <param name="identifier">An identifier.</param>
        /// <returns>The quoted identifier</returns>
        /// </summary>
        public string Quote(string identifier)
        {
            return "[" + identifier.Replace("]", "]]") + "]";
        }

        /// <summary>
        /// The character to escape quote (') character in string
        /// </summary>
        public char CharToEscapeQuote { get { return '\''; } }

        /// <summary>
        /// Create and run a batch delete statement.
        /// </summary>
        /// <typeparam name="TEntity">The type of the entity.</typeparam>
        /// <param name="objectContext">The <see cref="ObjectContext"/> to get connection and metadata information from.</param>
        /// <param name="entityMap">The <see cref="EntityMap"/> for <typeparamref name="TEntity"/>.</param>
        /// <param name="query">The query to create the where clause from.</param>
        /// <returns>
        /// The number of rows deleted.
        /// </returns>
        public int Delete<TEntity>(ObjectContext objectContext, EntityMap entityMap, ObjectQuery<TEntity> query) where TEntity : class
        {
#if NET45
            return InternalDelete(objectContext, entityMap, query, false).Result;
#else
            return InternalDelete(objectContext, entityMap, query);
#endif
        }

#if NET45
        /// <summary>
        /// Create and run a batch delete statement asynchronously.
        /// </summary>
        /// <typeparam name="TEntity">The type of the entity.</typeparam>
        /// <param name="objectContext">The <see cref="ObjectContext"/> to get connection and metadata information from.</param>
        /// <param name="entityMap">The <see cref="EntityMap"/> for <typeparamref name="TEntity"/>.</param>
        /// <param name="query">The query to create the where clause from.</param>
        /// <returns>
        /// The number of rows deleted.
        /// </returns>
        public Task<int> DeleteAsync<TEntity>(ObjectContext objectContext, EntityMap entityMap, ObjectQuery<TEntity> query) where TEntity : class
        {
            return InternalDelete(objectContext, entityMap, query, true);
        }
#endif

#if NET45
        private async Task<int> InternalDelete<TEntity>(ObjectContext objectContext, EntityMap entityMap, ObjectQuery<TEntity> query, bool async = false)
            where TEntity : class
#else
        private int InternalDelete<TEntity>(ObjectContext objectContext, EntityMap entityMap, ObjectQuery<TEntity> query)
            where TEntity : class
#endif
        {
            using (var db = QueryHelper.GetDb(objectContext))
            {

                var innerSelect = QueryHelper.GetSelectKeySql(query, entityMap, db.Command, this);

                var sqlBuilder = new StringBuilder(innerSelect.Length * 2);

                sqlBuilder.Append("DELETE ");
                sqlBuilder.Append(entityMap.TableName);
                sqlBuilder.AppendLine();

                sqlBuilder.AppendFormat("FROM {0} AS j0 INNER JOIN (", entityMap.TableName);
                sqlBuilder.AppendLine();
                sqlBuilder.AppendLine(innerSelect);
                sqlBuilder.Append(") AS j1 ON (");

                bool wroteKey = false;
                foreach (var keyMap in entityMap.KeyMaps)
                {
                    if (wroteKey)
                        sqlBuilder.Append(" AND ");

                    sqlBuilder.AppendFormat("j0.[{0}] = j1.[{0}]", keyMap.ColumnName);
                    wroteKey = true;
                }
                sqlBuilder.Append(")");

                db.Command.CommandText = sqlBuilder.ToString();
                db.Log(db.Command.CommandText);

#if NET45
                int result = async
                    ? await db.Command.ExecuteNonQueryAsync().ConfigureAwait(false)
                    : db.Command.ExecuteNonQuery();
#else
                int result = db.Command.ExecuteNonQuery();
#endif
                // only commit if created transaction
                if (db.OwnTransaction)
                    db.Transaction.Commit();

                return result;
            }
        }

        /// <summary>
        /// Create and run a batch update statement.
        /// </summary>
        /// <typeparam name="TEntity">The type of the entity.</typeparam>
        /// <param name="objectContext">The <see cref="ObjectContext"/> to get connection and metadata information from.</param>
        /// <param name="entityMap">The <see cref="EntityMap"/> for <typeparamref name="TEntity"/>.</param>
        /// <param name="query">The query to create the where clause from.</param>
        /// <param name="updateExpression">The update expression.</param>
        /// <returns>
        /// The number of rows updated.
        /// </returns>
        public int Update<TEntity>(ObjectContext objectContext, EntityMap entityMap, ObjectQuery<TEntity> query, Expression<Func<TEntity, TEntity>> updateExpression) where TEntity : class
        {
#if NET45
            return InternalUpdate(objectContext, entityMap, query, updateExpression, false).Result;
#else
            return InternalUpdate(objectContext, entityMap, query, updateExpression);
#endif
        }

#if NET45
        /// <summary>
        /// Create and run a batch update statement asynchronously.
        /// </summary>
        /// <typeparam name="TEntity">The type of the entity.</typeparam>
        /// <param name="objectContext">The <see cref="ObjectContext"/> to get connection and metadata information from.</param>
        /// <param name="entityMap">The <see cref="EntityMap"/> for <typeparamref name="TEntity"/>.</param>
        /// <param name="query">The query to create the where clause from.</param>
        /// <param name="updateExpression">The update expression.</param>
        /// <returns>
        /// The number of rows updated.
        /// </returns>
        public Task<int> UpdateAsync<TEntity>(ObjectContext objectContext, EntityMap entityMap, ObjectQuery<TEntity> query, Expression<Func<TEntity, TEntity>> updateExpression) where TEntity : class
        {
            return InternalUpdate(objectContext, entityMap, query, updateExpression, true);
        }
#endif
#if NET45
        private async Task<int> InternalUpdate<TEntity>(ObjectContext objectContext, EntityMap entityMap, ObjectQuery<TEntity> query, Expression<Func<TEntity, TEntity>> updateExpression, bool async = false)
            where TEntity : class
#else
        private int InternalUpdate<TEntity>(ObjectContext objectContext, EntityMap entityMap, ObjectQuery<TEntity> query, Expression<Func<TEntity, TEntity>> updateExpression, bool async = false)
            where TEntity : class
#endif
        {
            using (var db = QueryHelper.GetDb(objectContext))
            {

                var innerSelect = QueryHelper.GetSelectKeySql(query, entityMap, db.Command, this);
                var sqlBuilder = new StringBuilder(innerSelect.Length * 2);

                sqlBuilder.Append("UPDATE ");
                sqlBuilder.Append(entityMap.TableName);
                sqlBuilder.AppendLine(" SET ");

                var memberInitExpression = updateExpression.Body as MemberInitExpression;
                if (memberInitExpression == null)
                    throw new ArgumentException("The update expression must be of type MemberInitExpression.", "updateExpression");

                int nameCount = 0;
                bool wroteSet = false;
                foreach (MemberBinding binding in memberInitExpression.Bindings)
                {
                    if (wroteSet)
                        sqlBuilder.AppendLine(", ");

                    string propertyName = binding.Member.Name;
                    string columnName = entityMap.PropertyMaps
                        .Where(p => p.PropertyName == propertyName)
                        .Select(p => p.ColumnName)
                        .FirstOrDefault();


                    var memberAssignment = binding as MemberAssignment;
                    if (memberAssignment == null)
                        throw new ArgumentException("The update expression MemberBinding must only by type MemberAssignment.", "updateExpression");

                    Expression memberExpression = memberAssignment.Expression;

                    ParameterExpression parameterExpression = null;
                    memberExpression.Visit((ParameterExpression p) =>
                    {
                        if (p.Type == entityMap.EntityType)
                            parameterExpression = p;

                        return p;
                    });


                    if (parameterExpression == null)
                    {
                        object value;

                        if (memberExpression.NodeType == ExpressionType.Constant)
                        {
                            var constantExpression = memberExpression as ConstantExpression;
                            if (constantExpression == null)
                                throw new ArgumentException(
                                    "The MemberAssignment expression is not a ConstantExpression.", "updateExpression");

                            value = constantExpression.Value;
                        }
                        else
                        {
                            LambdaExpression lambda = Expression.Lambda(memberExpression, null);
                            value = lambda.Compile().DynamicInvoke();
                        }

                        if (value != null)
                        {
                            string parameterName = "p__update__" + nameCount++;
                            var parameter = db.Command.CreateParameter();
                            parameter.ParameterName = parameterName;
                            parameter.Value = value;
                            db.Command.Parameters.Add(parameter);

                            sqlBuilder.AppendFormat("[{0}] = @{1}", columnName, parameterName);
                        }
                        else
                        {
                            sqlBuilder.AppendFormat("[{0}] = NULL", columnName);
                        }
                    }
                    else
                    {
                        // create clean objectset to build query from
                        var objectSet = objectContext.CreateObjectSet<TEntity>();

                        Type[] typeArguments = new[] { entityMap.EntityType, memberExpression.Type };

                        ConstantExpression constantExpression = Expression.Constant(objectSet);
                        LambdaExpression lambdaExpression = Expression.Lambda(memberExpression, parameterExpression);

                        MethodCallExpression selectExpression = Expression.Call(
                            typeof(Queryable),
                            "Select",
                            typeArguments,
                            constantExpression,
                            lambdaExpression);

                        // create query from expression
                        var selectQuery = objectSet.CreateQuery(selectExpression, entityMap.EntityType);
                        string sql = selectQuery.ToTraceString();

                        // parse select part of sql to use as update
                        string regex = @"SELECT\s*\r\n\s*(?<ColumnValue>.+)?\s*AS\s*(?<ColumnAlias>\[\w+\])\r\n\s*FROM\s*(?<TableName>\[\w+\]\.\[\w+\]|\[\w+\])\s*AS\s*(?<TableAlias>\[\w+\])";
                        Match match = Regex.Match(sql, regex);
                        if (!match.Success)
                            throw new ArgumentException("The MemberAssignment expression could not be processed.", "updateExpression");

                        string value = match.Groups["ColumnValue"].Value;
                        string alias = match.Groups["TableAlias"].Value;

                        value = value.Replace(alias + ".", "j0.");

                        foreach (ObjectParameter objectParameter in selectQuery.Parameters)
                        {
                            string parameterName = "p__update__" + nameCount++;

                            var parameter = db.Command.CreateParameter();
                            parameter.ParameterName = parameterName;
                            parameter.Value = objectParameter.Value ?? DBNull.Value;
                            db.Command.Parameters.Add(parameter);

                            value = value.Replace(objectParameter.Name, parameterName);
                        }
                        sqlBuilder.AppendFormat("[{0}] = {1}", columnName, value);
                    }
                    wroteSet = true;
                }

                sqlBuilder.AppendLine(" ");
                sqlBuilder.AppendFormat("FROM {0} AS j0 INNER JOIN (", entityMap.TableName);
                sqlBuilder.AppendLine();
                sqlBuilder.AppendLine(innerSelect);
                sqlBuilder.Append(") AS j1 ON (");

                bool wroteKey = false;
                foreach (var keyMap in entityMap.KeyMaps)
                {
                    if (wroteKey)
                        sqlBuilder.Append(" AND ");

                    sqlBuilder.AppendFormat("j0.[{0}] = j1.[{0}]", keyMap.ColumnName);
                    wroteKey = true;
                }
                sqlBuilder.Append(")");

                db.Command.CommandText = sqlBuilder.ToString();
                db.Log(db.Command.CommandText);

#if NET45
                int result = async
                    ? await db.Command.ExecuteNonQueryAsync().ConfigureAwait(false)
                    : db.Command.ExecuteNonQuery();
#else
                int result = db.Command.ExecuteNonQuery();
#endif
                // only commit if created transaction
                if (db.OwnTransaction)
                    db.Transaction.Commit();

                return result;
            }
        }

        private int InternalUpdate<TModel>(IQueryable<TModel> query, ObjectQuery<TModel> objectQuery, EntityMap entityMap, Func<QueryHelper.Db, int> updateCommand) where TModel : class
        {
            var param = this.UpdateJoinParam(query, objectQuery, entityMap);
            using (var db = param.Item1)
            {
                string keySelect = param.Item2;
                var selectKeyFields = param.Item3;
                var idFieldsMap = param.Item4;
                string selectSql = param.Item5;
                var selectFields = param.Item6;
                var updateFields = param.Item7;

                string aliasTableUpdate = idFieldsMap.First().Value.Split('.')[0];
                var sqlFrom = selectSql.Substring(selectFields.FromIndex);
                string strRegex = @"\s+AS\s+" + Regex.Escape(aliasTableUpdate),
                       strRegex2 = Regex.Escape(entityMap.TableName) + strRegex;
                if (!Regex.IsMatch(sqlFrom, strRegex2, RegexOptions.IgnoreCase))
                {
                    var match = Regex.Match(sqlFrom, strRegex, RegexOptions.IgnoreCase);
                    if (match != null && match.Success)
                    {
                        int nextLine = sqlFrom.IndexOf('\n', match.Index + match.Length);
                        if (nextLine < 0) nextLine = sqlFrom.Length; else nextLine++;
                        aliasTableUpdate = Quote("__alias1");
                        var joinUpdate = new StringBuilder(" JOIN ").Append(entityMap.TableName).Append(" AS ").Append(aliasTableUpdate);
                        string conjunction = " ON ";
                        foreach (var kvp in idFieldsMap)
                        {
                            joinUpdate.Append(conjunction).Append(kvp.Value).Append(" = ")
                                .Append(aliasTableUpdate).Append(".")
                                .Append(entityMap.PropertyMaps.Where(p => p.PropertyName == kvp.Key).Select(p => Quote(p.ColumnName)).First());
                            conjunction = " AND ";
                        }
                        joinUpdate.AppendLine();
                        sqlFrom = sqlFrom.Insert(nextLine, joinUpdate.ToString());
                    }
                    else
                    {
                        throw new ArgumentException("Cannot read the updated table in the query", "query");
                    }
                }

                var sqlBuilder = new StringBuilder(selectSql.Length * 2);
                sqlBuilder.Append("UPDATE ").Append(aliasTableUpdate).Append(" SET").Append(Environment.NewLine);
                for (int i = 0; i < updateFields.Count; i++)
                {
                    if (updateFields[i] == null || idFieldsMap.ContainsKey(updateFields[i])) continue;
                    string valueField = selectFields[i];
                    if (selectFields.AliasIndexes[i] > 0) valueField = valueField.Substring(0, selectFields.AliasIndexes[i]);
                    sqlBuilder.Append(entityMap.PropertyMaps.Where(p => p.PropertyName == updateFields[i]).Select(p => Quote(p.ColumnName)).First())
                        .Append(" = ").Append(valueField);
                    if (i < updateFields.Count - 1) sqlBuilder.Append(",");
                    sqlBuilder.AppendLine();
                }
                sqlBuilder.Append(sqlFrom);

                db.Command.CommandText = sqlBuilder.ToString();
                db.Log(db.Command.CommandText);
                int result = updateCommand(db);

                // only commit if created transaction
                if (db.OwnTransaction)
                    db.Transaction.Commit();
                return result;
            }
        }

        /// <summary>
        /// Execute statement `<code>UPDATE [Table] SET ... FROM [Table] {JOIN Another Table ...} ...</code>`.
        /// </summary>
        /// <typeparam name="TModel">The type <paramref name="query"/> item.</typeparam>
        /// <param name="query">The SELECT query from which we determine what field to be set (updated). It must include primary key(s) field so that the update statement
        /// will appropriately update the true records. The key field(s) will not updated.</param>
        /// <param name="objectQuery">The query to create SQL statement and it can also be used to get the information of db connection via
        ///     <code>objectQuery.Context</code> property.</param>
        /// <param name="entityMap">The <see cref="EntityMap"/> for entity type of the updated table (<code>IDbSet</code>).</param>
        /// <returns>
        /// The number of rows updated.
        /// </returns>
        public int Update<TModel>(IQueryable<TModel> query, ObjectQuery<TModel> objectQuery, EntityMap entityMap)
            where TModel : class
        {
            return InternalUpdate(query, objectQuery, entityMap,
                db => db.Command.ExecuteNonQuery());
        }

#if NET45
        /// <summary>
        /// Execute statement `<code>UPDATE [Table] SET ... FROM [Table] {JOIN Another Table ...} ...</code>`.
        /// </summary>
        /// <typeparam name="TModel">The type <paramref name="query"/> item.</typeparam>
        /// <param name="query">The SELECT query from which we determine what field to be set (updated). It must include primary key(s) field so that the update statement
        /// will appropriately update the true records. The key field(s) will not updated.</param>
        /// <param name="objectQuery">The query to create SQL statement and it can also be used to get the information of db connection via
        ///     <code>objectQuery.Context</code> property.</param>
        /// <param name="entityMap">The <see cref="EntityMap"/> for entity type of the updated table (<code>IDbSet</code>).</param>
        /// <returns>
        /// The number of rows updated.
        /// </returns>
        public Task<int> UpdateAsync<TModel>(IQueryable<TModel> query, ObjectQuery<TModel> objectQuery, EntityMap entityMap)
            where TModel : class
        {
            Func<QueryHelper.Db, Task<int>> update = async (QueryHelper.Db db) =>
            {
                return await db.Command.ExecuteNonQueryAsync();
            };
            return new Task<int>( () =>
                InternalUpdate(query, objectQuery, entityMap, db => update(db).Result)
            );
        }
#endif

        /// <summary>
        /// Execute statement `<code>INSERT INTO [Table] (...) SELECT ...</code>`.
        /// </summary>
        /// <typeparam name="TModel">The type <paramref name="query"/> item.</typeparam>
        /// <param name="query">The query to create SELECT clause statement.</param>
        /// <param name="objectQuery">The query to create SELECT clause statement and it can also be used to get the information of db connection via
        ///     <code>objectQuery.Context</code> property.</param>
        /// <param name="entityMap">The <see cref="EntityMap"/> for entity type of the destination table (<code>IDbSet</code>).</param>
        /// <returns>
        /// The number of rows inserted.
        /// </returns>
        public int Insert<TModel>(IQueryable<TModel> query, ObjectQuery<TModel> objectQuery, EntityMap entityMap)
            where TModel : class
        {
#if NET45
            return this.InternalInsert(query, objectQuery, entityMap, false).Result;
#else
            return this.InternalInsert(query, objectQuery, entityMap);
#endif
        }

#if NET45
        /// <summary>
        /// Execute statement `<code>INSERT INTO [Table] (...) SELECT ...</code>`.
        /// </summary>
        /// <typeparam name="TModel">The type <paramref name="query"/> item.</typeparam>
        /// <param name="query">The query to create SELECT clause statement.</param>
        /// <param name="objectQuery">The query to create SELECT clause statement and it can also be used to get the information of db connection via
        ///     <code>objectQuery.Context</code> property.</param>
        /// <param name="entityMap">The <see cref="EntityMap"/> for entity type of the destination table (<code>IDbSet</code>).</param>
        /// <returns>
        /// The number of rows inserted.
        /// </returns>
        public Task<int> InsertAsync<TModel>(IQueryable<TModel> query, ObjectQuery<TModel> objectQuery, EntityMap entityMap)
            where TModel : class
        {
            return this.InternalInsert(query, objectQuery, entityMap, true);
        }
#endif

        private SqlBulkCopy CreateSqlBulkCopy(ObjectContext objectContext, EntityMap entityMap, int batchSize, int timeout)
        {
            var dbConn = (objectContext.Connection as EntityConnection).StoreConnection as SqlConnection;
            if (dbConn.State == ConnectionState.Closed) dbConn.Open();
            SqlTransaction dbTrans = null;
            try
            {
                dbTrans = objectContext.TransactionHandler.DbContext.Database.CurrentTransaction.UnderlyingTransaction as SqlTransaction;
            }
            catch (NullReferenceException)
            {
            }
            var sqlBulkCopy = new SqlBulkCopy((objectContext.Connection as EntityConnection).StoreConnection as SqlConnection,
                SqlBulkCopyOptions.Default, dbTrans);
            sqlBulkCopy.BatchSize = batchSize;
            sqlBulkCopy.BulkCopyTimeout = timeout;
            sqlBulkCopy.DestinationTableName = entityMap.TableName;
            return sqlBulkCopy;
        }

        /// <summary>
        /// Inserts a lof of rows into a database table. It must be much faster than executing `<code>DbSet.AddRange</code>` or
        /// repetitive `<code>DbSet.Add</code>` method and then executing '<code>DbContext.SaveChanges</code>' method. It uses
        /// <code>SqlBulkCopy</code> class.
        /// </summary>
        /// <typeparam name="TEntity">The type of objects representing rows to be inserted into db table.</typeparam>
        /// <param name="objectContext">The <code>ObjectContext</code> object corresponding to the database table to which the rows will be inserted.</param>
        /// <param name="entities">The entity objects reprsenting the rows that will be inserted.</param>
        /// <param name="entityMap">The <see cref="EntityMap"/> for entity type of the destination table.</param>
        /// <param name="batchSize">Number of rows in each batch. At the end of each batch, the rows in the batch are sent to the server. Zero means there
        /// will be a single batch</param>
        /// <param name="timeout">Number of seconds for the operation to complete before it times out. Zero means no limit.</param>
        /// <returns>
        /// The number of rows inserted.
        /// </returns>
        public int Insert<TEntity>(ObjectContext objectContext, IEnumerable<TEntity> entities, EntityMap entityMap, int batchSize, int timeout)
            where TEntity : class
        {
            var dataReader = new EntityDataReader<TEntity>(entities, entityMap);
            var sqlBulkCopy = CreateSqlBulkCopy(objectContext, entityMap, batchSize, timeout);
            sqlBulkCopy.WriteToServer(dataReader);
            return entities.Count();
        }

#if NET45
        /// <summary>
        /// Inserts a lof of rows into a database table asynchronously. It must be much faster than executing `<code>DbSet.AddRange</code>` or
        /// repetitive `<code>DbSet.Add</code>` method and then executing '<code>DbContext.SaveChanges</code>' method. It uses
        /// <code>SqlBulkCopy</code> class.
        /// </summary>
        /// <typeparam name="TEntity">The type of objects representing rows to be inserted into db table.</typeparam>
        /// <param name="objectContext">The <code>ObjectContext</code> object corresponding to the database table to which the rows will be inserted.</param>
        /// <param name="entities">The entity objects reprsenting the rows that will be inserted.</param>
        /// <param name="entityMap">The <see cref="EntityMap"/> for entity type of the destination table.</param>
        /// <param name="batchSize">Number of rows in each batch. At the end of each batch, the rows in the batch are sent to the server. Zero means there
        /// will be a single batch</param>
        /// <param name="timeout">Number of seconds for the operation to complete before it times out. Zero means no limit.</param>
        /// <returns>
        /// The number of rows inserted.
        /// </returns>
        public async Task<int> InsertAsync<TEntity>(ObjectContext objectContext, IEnumerable<TEntity> entities, EntityMap entityMap, int batchSize, int timeout)
            where TEntity : class
        {
            var dataReader = new EntityDataReader<TEntity>(entities, entityMap);
            var sqlBulkCopy = CreateSqlBulkCopy(objectContext, entityMap, batchSize, timeout);
            await sqlBulkCopy.WriteToServerAsync(dataReader);
            return entities.Count();
        }
#endif
    }
}