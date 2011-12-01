using System;
using System.Collections.Generic;
using System.Data.Common;
using System.Data.Common.CommandTrees;
using System.Data.Entity;
using System.Data.EntityClient;
using System.Data.Metadata.Edm;
using System.Data.Objects.DataClasses;
using System.Data.SqlClient;
using System.Linq;
using System.Linq.Dynamic;
using System.Linq.Expressions;
using System.Reflection;
using System.Text;
using System.Data.Objects;
using EntityFramework.Mapping;
using EntityFramework.Reflection;

namespace EntityFramework.Extensions
{
    public static class BatchExtensions
    {
        public static int Delete<TEntity>(
            this ObjectSet<TEntity> source,
            IQueryable<TEntity> query)
            where TEntity : class
        {
            if (source == null)
                throw new ArgumentNullException("source");

            if (query == null)
                throw new ArgumentNullException("query");

            ObjectContext objectContext = source.Context;
            if (objectContext == null)
                throw new ArgumentException("The ObjectContext for the source query can not be null.", "source");

            EntityMap entityMap = source.GetEntityMap<TEntity>();
            if (entityMap == null)
                throw new ArgumentException("Could not load the entity mapping information for the source ObjectSet.", "source");

            ObjectQuery<TEntity> objectQuery = query.ToObjectQuery();
            if (objectQuery == null)
                throw new ArgumentException("The query must be of type ObjectQuery or DbQuery.", "query");

            return Delete(objectContext, entityMap, objectQuery);
        }

        public static int Delete<TEntity>(
            this ObjectSet<TEntity> source,
            Expression<Func<TEntity, bool>> filterExpression)
            where TEntity : class
        {
            if (source == null)
                throw new ArgumentNullException("source");
            if (filterExpression == null)
                throw new ArgumentNullException("filterExpression");

            return source.Delete(source.Where(filterExpression));
        }

        public static int Delete<TEntity>(
           this DbSet<TEntity> source,
           IQueryable<TEntity> query)
           where TEntity : class
        {
            if (source == null)
                throw new ArgumentNullException("source");
            if (query == null)
                throw new ArgumentNullException("query");

            ObjectQuery<TEntity> sourceQuery = source.ToObjectQuery();
            if (sourceQuery == null)
                throw new ArgumentException("The query must be of type ObjectQuery or DbQuery.", "source");

            ObjectContext objectContext = sourceQuery.Context;
            if (objectContext == null)
                throw new ArgumentException("The ObjectContext for the source query can not be null.", "source");

            EntityMap entityMap = sourceQuery.GetEntityMap<TEntity>();
            if (entityMap == null)
                throw new ArgumentException("Could not load the entity mapping information for the source ObjectSet.", "source");

            ObjectQuery<TEntity> objectQuery = query.ToObjectQuery();
            if (objectQuery == null)
                throw new ArgumentException("The query must be of type ObjectQuery or DbQuery.", "query");

            return Delete(objectContext, entityMap, objectQuery);
        }

        public static int Delete<TEntity>(
            this DbSet<TEntity> source,
            Expression<Func<TEntity, bool>> filterExpression)
            where TEntity : class
        {
            if (source == null)
                throw new ArgumentNullException("source");
            if (filterExpression == null)
                throw new ArgumentNullException("filterExpression");

            return source.Delete(source.Where(filterExpression));
        }


        public static int Update<TEntity>(
            this ObjectSet<TEntity> source,
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

            ObjectContext objectContext = source.Context;
            if (objectContext == null)
                throw new ArgumentException("The ObjectContext for the source query can not be null.", "source");

            EntityMap entityMap = source.GetEntityMap<TEntity>();
            if (entityMap == null)
                throw new ArgumentException("Could not load the entity mapping information for the source ObjectSet.", "source");

            ObjectQuery<TEntity> objectQuery = query.ToObjectQuery();
            if (objectQuery == null)
                throw new ArgumentException("The query must be of type ObjectQuery or DbQuery.", "query");

            return Update(objectContext, entityMap, objectQuery, updateExpression);
        }

        public static int Update<TEntity>(
            this ObjectSet<TEntity> source,
            Expression<Func<TEntity, bool>> filterExpression,
            Expression<Func<TEntity, TEntity>> updateExpression)
            where TEntity : class
        {
            if (source == null)
                throw new ArgumentNullException("source");
            if (filterExpression == null)
                throw new ArgumentNullException("filterExpression");

            return source.Update(source.Where(filterExpression), updateExpression);
        }

        public static int Update<TEntity>(
            this DbSet<TEntity> source,
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

            return Update(objectContext, entityMap, objectQuery, updateExpression);
        }

        public static int Update<TEntity>(
            this DbSet<TEntity> source,
            Expression<Func<TEntity, bool>> filterExpression,
            Expression<Func<TEntity, TEntity>> updateExpression)
            where TEntity : class
        {
            if (source == null)
                throw new ArgumentNullException("source");
            if (filterExpression == null)
                throw new ArgumentNullException("filterExpression");

            return source.Update(source.Where(filterExpression), updateExpression);
        }


        private static int Delete<TEntity>(ObjectContext objectContext, EntityMap entityMap, ObjectQuery<TEntity> query)
            where TEntity : class
        {
            DbConnection deleteConnection = null;
            DbTransaction deleteTransaction = null;
            DbCommand deleteCommand = null;

            try
            {
                deleteConnection = GetStoreConnection(objectContext);
                deleteConnection.Open();

                deleteTransaction = deleteConnection.BeginTransaction();

                deleteCommand = deleteConnection.CreateCommand();
                deleteCommand.Transaction = deleteTransaction;
                if (objectContext.CommandTimeout.HasValue)
                    deleteCommand.CommandTimeout = objectContext.CommandTimeout.Value;

                var innerSelect = GetSelectSql(query, entityMap, deleteCommand);

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

                    sqlBuilder.AppendFormat("j0.{0} = j1.{0}", keyMap.ColumnName);
                    wroteKey = true;
                }
                sqlBuilder.Append(")");

                deleteCommand.CommandText = sqlBuilder.ToString();

                int result = deleteCommand.ExecuteNonQuery();
                deleteTransaction.Commit();

                return result;
            }
            finally
            {
                if (deleteCommand != null)
                    deleteCommand.Dispose();
                if (deleteTransaction != null)
                    deleteTransaction.Dispose();
                if (deleteConnection != null)
                    deleteConnection.Dispose();
            }
        }

        private static int Update<TEntity>(ObjectContext objectContext, EntityMap entityMap, ObjectQuery<TEntity> query, Expression<Func<TEntity, TEntity>> updateExpression)
            where TEntity : class
        {
            DbConnection updateConnection = null;
            DbTransaction updateTransaction = null;
            DbCommand updateCommand = null;

            try
            {
                updateConnection = GetStoreConnection(objectContext);
                updateConnection.Open();

                updateTransaction = updateConnection.BeginTransaction();

                updateCommand = updateConnection.CreateCommand();
                updateCommand.Transaction = updateTransaction;
                if (objectContext.CommandTimeout.HasValue)
                    updateCommand.CommandTimeout = objectContext.CommandTimeout.Value;

                var innerSelect = GetSelectSql(query, entityMap, updateCommand);
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

                    string parameterName = "p__update__" + nameCount++;

                    var memberAssignment = binding as MemberAssignment;
                    if (memberAssignment == null)
                        throw new ArgumentException("The update expression MemberBinding must only by type MemberAssignment.", "updateExpression");

                    object value;
                    if (memberAssignment.Expression.NodeType == ExpressionType.Constant)
                    {
                        var constantExpression = memberAssignment.Expression as ConstantExpression;
                        if (constantExpression == null)
                            throw new ArgumentException("The MemberAssignment expression is not a ConstantExpression.", "updateExpression");

                        value = constantExpression.Value;
                    }
                    else
                    {
                        LambdaExpression lambda = Expression.Lambda(memberAssignment.Expression, null);
                        value = lambda.Compile().DynamicInvoke();
                    }

                    var parameter = updateCommand.CreateParameter();
                    parameter.ParameterName = parameterName;
                    parameter.Value = value;
                    updateCommand.Parameters.Add(parameter);

                    sqlBuilder.AppendFormat("{0} = @{1}", columnName, parameterName);
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

                    sqlBuilder.AppendFormat("j0.{0} = j1.{0}", keyMap.ColumnName);
                    wroteKey = true;
                }
                sqlBuilder.Append(")");

                updateCommand.CommandText = sqlBuilder.ToString();

                int result = updateCommand.ExecuteNonQuery();
                updateTransaction.Commit();

                return result;
            }
            finally
            {
                if (updateCommand != null)
                    updateCommand.Dispose();
                if (updateTransaction != null)
                    updateTransaction.Dispose();
                if (updateConnection != null)
                    updateConnection.Dispose();
            }
        }

        private static DbConnection GetStoreConnection(ObjectContext objectContext)
        {
            DbConnection dbConnection = objectContext.Connection;
            var entityConnection = dbConnection as EntityConnection;

            // by-pass entity connection
            var connection = entityConnection == null
                ? dbConnection
                : entityConnection.StoreConnection;

            return connection;
        }

        private static string GetSelectSql<TEntity>(ObjectQuery<TEntity> query, EntityMap entityMap, DbCommand command)
            where TEntity : class
        {
            // changing query to only select keys
            var selector = new StringBuilder(50);
            selector.Append("new(");
            foreach (var propertyMap in entityMap.KeyMaps)
            {
                if (selector.Length > 4)
                    selector.Append((", "));

                selector.Append(propertyMap.PropertyName);
            }
            selector.Append(")");

            var selectQuery = DynamicQueryable.Select(query, selector.ToString());
            var objectQuery = selectQuery as ObjectQuery;

            if (objectQuery == null)
                throw new ArgumentException("The query must be of type ObjectQuery.", "query");

            string innerJoinSql = objectQuery.ToTraceString();

            // create parameters
            foreach (var objectParameter in objectQuery.Parameters)
            {
                var parameter = command.CreateParameter();
                parameter.ParameterName = objectParameter.Name;
                parameter.Value = objectParameter.Value;

                command.Parameters.Add(parameter);
            }

            return innerJoinSql;
        }

    }
}
