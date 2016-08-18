using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Common;
using System.Data.Entity.Core.EntityClient;
using System.Data.Entity.Core.Objects;
using System.Globalization;
using System.Linq;
using System.Linq.Dynamic;
using System.Linq.Expressions;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using EntityFramework.Batch;
using EntityFramework.Extensions;
using EntityFramework.Mapping;
using EntityFramework.Reflection;


namespace EntityFramework.Batch
{
    /// <summary>
    /// A batch execution runner for Oracle.
    /// </summary>
    public class OracleBatchRunner : IBatchRunner
    {
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
            return InternalDelete(objectContext, entityMap, query).Result;
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
            DbConnection deleteConnection = null;
            DbTransaction deleteTransaction = null;
            bool ownConnection = false;
            bool ownTransaction = false;

            try
            {
                InitializeConnectionAndTransaction(objectContext, ref deleteConnection, ref deleteTransaction, ref ownConnection, ref ownTransaction);

                using (var deleteCommand = CreateCommand(objectContext, deleteConnection, deleteTransaction))
                {
                    deleteCommand.Transaction = deleteTransaction;
                    if (objectContext.CommandTimeout.HasValue)
                    {
                        deleteCommand.CommandTimeout = objectContext.CommandTimeout.Value;
                    }

                    var innerSelect = GetSelectSql(query, entityMap, deleteCommand);
                    var sqlBuilder = new StringBuilder();
                    sqlBuilder.Append("DELETE ");
                    sqlBuilder.AppendLine(entityMap.TableName.Replace('[', '\"').Replace(']', '\"'));
                    sqlBuilder.AppendLine("WHERE  ROWID IN");
                    sqlBuilder.AppendLine("(");
                    sqlBuilder.AppendLine("SELECT \"Extent1\".ROWID");
                    sqlBuilder.AppendLine(innerSelect.Substring(innerSelect.IndexOf("FROM")));
                    sqlBuilder.AppendLine(")");

                    deleteCommand.CommandText = sqlBuilder.ToString();

#if NET45
                    int result = async
                        ? await deleteCommand.ExecuteNonQueryAsync().ConfigureAwait(false)
                        : deleteCommand.ExecuteNonQuery();
#else
                    int result = deleteCommand.ExecuteNonQuery();
#endif

                    return result;
                }
            }
            finally
            {
                ReleaseConnectionAndTransaction(deleteConnection, deleteTransaction, ownConnection, ownTransaction);
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
            DbConnection updateConnection = null;
            DbTransaction updateTransaction = null;
            bool ownConnection = false;
            bool ownTransaction = false;

            try
            {
                InitializeConnectionAndTransaction(objectContext, ref updateConnection, ref updateTransaction, ref ownConnection, ref ownTransaction);

                using (var updateCommand = CreateCommand(objectContext, updateConnection, updateTransaction))
                {
                    var memberInitExpression = updateExpression.Body as MemberInitExpression;
                    if (memberInitExpression == null)
                    {
                        throw new ArgumentException("The update expression must be of type MemberInitExpression.", "updateExpression");
                    }

                    var innerSelect = GetSelectSql(query, entityMap, updateCommand);
                    var sqlBuilder = BuildUpdateSql<TEntity>(objectContext, entityMap, updateCommand, innerSelect, memberInitExpression);
                    updateCommand.CommandText = sqlBuilder.ToString();

#if NET45
                    int result = async
                        ? await updateCommand.ExecuteNonQueryAsync().ConfigureAwait(false)
                        : updateCommand.ExecuteNonQuery();
#else
                    int result = updateCommand.ExecuteNonQuery();
#endif

                    return result;
                }
            }
            finally
            {
                ReleaseConnectionAndTransaction(updateConnection, updateTransaction, ownConnection, ownTransaction);
            }
        }

        #region Connection & Transaction Management

        private static Tuple<DbConnection, DbTransaction> GetStore(ObjectContext objectContext)
        {
            var dbConnection = objectContext.Connection;
            var entityConnection = dbConnection as EntityConnection;

            // by-pass entity connection
            if (entityConnection == null)
            {
                return new Tuple<DbConnection, DbTransaction>(dbConnection, null);
            }

            // get internal transaction
            var connection = entityConnection.StoreConnection;
            dynamic connectionProxy = new DynamicProxy(entityConnection);
            dynamic entityTransaction = connectionProxy.CurrentTransaction;
            if (entityTransaction == null)
            {
                return new Tuple<DbConnection, DbTransaction>(connection, null);
            }

            var transaction = entityTransaction.StoreTransaction;
            return new Tuple<DbConnection, DbTransaction>(connection, transaction);
        }

        private static void InitializeConnectionAndTransaction(ObjectContext objectContext, ref DbConnection connection, ref DbTransaction transaction, ref bool ownConnection, ref bool ownTransaction)
        {
            // get store connection and transaction
            var store = GetStore(objectContext);
            connection = store.Item1;
            transaction = store.Item2;

            if (connection.State != ConnectionState.Open)
            {
                connection.Open();
                ownConnection = true;
            }

            // use existing transaction or create new
            if (transaction == null)
            {
                transaction = connection.BeginTransaction();
                ownTransaction = true;
            }
        }

        private static DbCommand CreateCommand(ObjectContext objectContext, DbConnection connection, DbTransaction transaction)
        {
            var command = connection.CreateCommand();

            command.Transaction = transaction;
            if (objectContext.CommandTimeout.HasValue)
            {
                command.CommandTimeout = objectContext.CommandTimeout.Value;
            }

            return command;
        }


        private static void ReleaseConnectionAndTransaction(DbConnection connection, DbTransaction transaction, bool ownConnection, bool ownTransaction)
        {
            if (transaction != null && ownTransaction)
            {
                transaction.Dispose();
            }

            if (connection != null && ownConnection)
            {
                connection.Close();
            }
        }

        #endregion

        #region Update Helpers

        private static StringBuilder BuildUpdateSql<TEntity>(ObjectContext objectContext, EntityMap entityMap, DbCommand updateCommand, string innerSelect, MemberInitExpression memberInitExpression) where TEntity : class
        {
            int nameCount = 0;
            bool wroteSet = false;
            var fieldsToUpdate = new StringBuilder();
            var valuesToUpdate = new StringBuilder();
            foreach (MemberBinding binding in memberInitExpression.Bindings)
            {
                var memberAssignment = binding as MemberAssignment;
                if (memberAssignment == null)
                {
                    throw new ArgumentException("The update expression MemberBinding must only by type MemberAssignment.", "updateExpression");
                }

                if (wroteSet)
                {
                    fieldsToUpdate.Append(", ");
                    valuesToUpdate.Append(", ");
                }

                string propertyName = binding.Member.Name;
                string columnName = entityMap.PropertyMaps.Where(p => p.PropertyName == propertyName)
                                                          .Select(p => p.ColumnName)
                                                          .FirstOrDefault();

                var memberExpression = memberAssignment.Expression;
                ParameterExpression parameterExpression = null;
                memberExpression.Visit((ParameterExpression p) =>
                {
                    if (p.Type == entityMap.EntityType)
                    {
                        parameterExpression = p;
                    }

                    return p;
                });

                if (parameterExpression == null)
                {
                    nameCount = BuildUpdateParameterWithExpression(updateCommand, fieldsToUpdate, valuesToUpdate, nameCount, columnName, memberExpression);
                }
                else
                {
                    nameCount = BuildUpdateParameterWithoutExpression<TEntity>(objectContext, entityMap, updateCommand, fieldsToUpdate, valuesToUpdate, nameCount, columnName, memberExpression, parameterExpression);
                }

                wroteSet = true;
            }

            var sqlBuilder = new StringBuilder();
            sqlBuilder.Append("UPDATE ");
            sqlBuilder.AppendLine(entityMap.TableName.Replace('[', '\"').Replace(']', '\"'));
            sqlBuilder.Append("SET (");
            sqlBuilder.Append(fieldsToUpdate);
            sqlBuilder.AppendLine(") = (");
            sqlBuilder.Append("SELECT ");
            sqlBuilder.Append(valuesToUpdate);
            sqlBuilder.AppendLine();
            sqlBuilder.AppendLine(innerSelect.Substring(innerSelect.IndexOf("FROM")));
            sqlBuilder.AppendLine(")");
            return sqlBuilder;
        }

        private static int BuildUpdateParameterWithExpression(DbCommand updateCommand, StringBuilder fieldsToUpdate, StringBuilder valuesToUpdate, int nameCount, string columnName, Expression memberExpression)
        {
            object value;

            if (memberExpression.NodeType == ExpressionType.Constant)
            {
                var constantExpression = memberExpression as ConstantExpression;
                if (constantExpression == null)
                {
                    throw new ArgumentException("The MemberAssignment expression is not a ConstantExpression.", "updateExpression");
                }

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
                var parameter = updateCommand.CreateParameter();
                parameter.ParameterName = parameterName;
                parameter.Value = value;
                updateCommand.Parameters.Add(parameter);

                fieldsToUpdate.AppendFormat("\"{0}\"", columnName);
                valuesToUpdate.AppendFormat(":{0}", parameterName);
            }
            else
            {
                fieldsToUpdate.AppendFormat("\"{0}\"", columnName);
                valuesToUpdate.Append("NULL");
            }

            return nameCount;
        }

        private static int BuildUpdateParameterWithoutExpression<TEntity>(ObjectContext objectContext, EntityMap entityMap, DbCommand updateCommand, StringBuilder fieldsToUpdate, StringBuilder valuesToUpdate, int nameCount, string columnName, Expression memberExpression, ParameterExpression parameterExpression) where TEntity : class
        {
            // create clean objectset to build query from
            var objectSet = objectContext.CreateObjectSet<TEntity>();
            var typeArguments = new[] { entityMap.EntityType, memberExpression.Type };
            var constantExpression = Expression.Constant(objectSet);
            var lambdaExpression = Expression.Lambda(memberExpression, parameterExpression);
            var selectExpression = Expression.Call(typeof(Queryable), "Select", typeArguments, constantExpression, lambdaExpression);

            // create query from expression
            var selectQuery = objectSet.CreateQuery(selectExpression, entityMap.EntityType);
            var sql = selectQuery.ToTraceString();

            // parse select part of sql to use as update
            var regex = @"SELECT\s*\r\n\s*(?<ColumnValue>.+)?\s*AS\s*(?<ColumnAlias>\[\w+\])\r\n\s*FROM\s*(?<TableName>\[\w+\]\.\[\w+\]|\[\w+\])\s*AS\s*(?<TableAlias>\[\w+\])";
            var match = Regex.Match(sql, regex);
            if (!match.Success)
            {
                throw new ArgumentException("The MemberAssignment expression could not be processed.", "updateExpression");
            }

            var alias = match.Groups["TableAlias"].Value;
            var value = match.Groups["ColumnValue"].Value.Replace(alias + ".", "");

            foreach (ObjectParameter objectParameter in selectQuery.Parameters)
            {
                var parameterName = "p__update__" + nameCount++;
                var parameter = updateCommand.CreateParameter();

                parameter.ParameterName = parameterName;
                parameter.Value = objectParameter.Value ?? DBNull.Value;
                updateCommand.Parameters.Add(parameter);

                value = value.Replace(objectParameter.Name, parameterName);
            }

            fieldsToUpdate.AppendFormat("\"{0}\"", columnName);
            valuesToUpdate.AppendFormat("\"{0}\"", value);

            return nameCount;
        }

        #endregion

        #region Select Builder

        private static string GetSelectSql<TEntity>(ObjectQuery<TEntity> query, EntityMap entityMap, DbCommand command)
            where TEntity : class
        {
            // changing query to only select keys
            var selector = new StringBuilder(50);
            selector.Append("new(");

            foreach (var propertyMap in entityMap.KeyMaps)
            {
                if (selector.Length > 4)
                {
                    selector.Append((", "));
                }

                selector.Append(propertyMap.PropertyName);
            }
            selector.Append(")");

            var selectQuery = DynamicQueryable.Select(query, selector.ToString());
            var objectQuery = selectQuery as ObjectQuery;

            if (objectQuery == null)
            {
                throw new ArgumentException("The query must be of type ObjectQuery.", "query");
            }

            var innerJoinSql = objectQuery.ToTraceString();

            // create parameters
            foreach (var objectParameter in objectQuery.Parameters)
            {
                var parameter = command.CreateParameter();
                parameter.ParameterName = objectParameter.Name;
                parameter.Value = objectParameter.Value ?? DBNull.Value;

                command.Parameters.Add(parameter);
            }

            return innerJoinSql;
        }

        #endregion
    }
}
