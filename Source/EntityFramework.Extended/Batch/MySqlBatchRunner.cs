using System;
using System.Data;
using System.Data.Common;
using System.Data.Entity.Core.EntityClient;
using System.Data.Entity.Core.Objects;
using System.Linq;
using System.Linq.Dynamic;
using System.Linq.Expressions;
using System.Text;
using System.Text.RegularExpressions;
using EntityFramework.Extensions;
using EntityFramework.Mapping;
using EntityFramework.Reflection;
using System.Threading.Tasks;
using System.Data.Entity;
using System.Data.Entity.Infrastructure.Interception;
using System.Data.SqlClient;

namespace EntityFramework.Batch
{
    /// <summary>
    /// A batch execution runner for MySQL Server.
    /// </summary>
    public class MySqlBatchRunner : IBatchRunner
    {
        /// <summary>
        /// NULL value for a parameter of <see cref="DbCommand"/>.
        /// </summary>
        public object DbNull { get { return null; } }

        /// <summary>
        /// To quote an SQL identifier so that it's safe to be included in an SQL statement
        /// <param name="identifier">An identifier.</param>
        /// <returns>The quoted identifier</returns>
        /// </summary>
        public string Quote(string identifier)
        {
            return "`" + identifier.Replace("`", "``") + "`";
        }

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
        public int Delete<TEntity>(ObjectContext objectContext, EntityMap entityMap, ObjectQuery<TEntity> query)
            where TEntity : class
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

                sqlBuilder.Append("DELETE j0");
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

                    sqlBuilder.AppendFormat("j0.{0} = j1.{0}", keyMap.ColumnName);
                    wroteKey = true;
                }
                sqlBuilder.Append(")");

                db.Command.CommandText = sqlBuilder.ToString();

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
                sqlBuilder.AppendFormat(" AS j0 INNER JOIN (", entityMap.TableName);
                sqlBuilder.AppendLine();
                sqlBuilder.AppendLine(innerSelect);
                sqlBuilder.Append(") AS j1 ON (");

                bool wroteKey = false;
                foreach (var keyMap in entityMap.KeyMaps)
                {
                    if (wroteKey)
                        sqlBuilder.Append(" AND ");

                    sqlBuilder.AppendFormat("j0.{0} = j1.{0}", keyMap.ColumnName);
                    wroteKey = true;
                }
                sqlBuilder.Append(")");
                sqlBuilder.AppendLine(" ");

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

                            sqlBuilder.AppendFormat("{0} = @{1}", columnName, parameterName);
                        }
                        else
                        {
                            sqlBuilder.AppendFormat("{0} = NULL", columnName);
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
                        string regex = @"SELECT\s*\r\n(?<ColumnValue>.+)?\s*AS\s*(?<ColumnAlias>\w+)\r\nFROM\s*(?<TableName>\w+\.\w+|\w+)\s*AS\s*(?<TableAlias>\w+)";
                        Match match = Regex.Match(sql, regex);
                        if (!match.Success)
                            throw new ArgumentException("The MemberAssignment expression could not be processed.", "updateExpression");

                        string value = match.Groups["ColumnValue"].Value;
                        string alias = match.Groups["TableAlias"].Value;

                        value = value.Replace(alias + ".", "");

                        foreach (ObjectParameter objectParameter in selectQuery.Parameters)
                        {
                            string parameterName = "p__update__" + nameCount++;

                            var parameter = db.Command.CreateParameter();
                            parameter.ParameterName = parameterName;
                            parameter.Value = objectParameter.Value;
                            db.Command.Parameters.Add(parameter);

                            value = value.Replace(objectParameter.Name, parameterName);
                        }
                        sqlBuilder.AppendFormat("{0} = {1}", columnName, value);
                    }
                    wroteSet = true;
                }


                db.Command.CommandText = sqlBuilder.ToString();

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
        public int Insert<TModel>(IQueryable<TModel> query, ObjectQuery<TModel> objectQuery, EntityMap entityMap) where TModel : class
        {
#if NET45
            return this.InternalInsert(query, objectQuery, entityMap, false).Result;
#else
            return this.InternalInsert(query, objectQuery, entityMap, false);
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
        /// <param name="entityMap">The <see cref="EntityMap"/> for entity type of the destination table (<see cref="IDbSet"/>).</param>
        /// <returns>
        /// The number of rows inserted.
        /// </returns>
        public Task<int> InsertAsync<TModel>(IQueryable<TModel> query, ObjectQuery<TModel> objectQuery, EntityMap entityMap) where TModel : class
        {
            return this.InternalInsert(query, objectQuery, entityMap, true);
        }
#endif
    }
}