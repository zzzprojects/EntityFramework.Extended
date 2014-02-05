using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Common;
using System.Data.Entity.Core.EntityClient;
using System.Data.Entity.Core.Objects;
using System.Linq;
using System.Linq.Dynamic;
using System.Linq.Expressions;
using System.Reflection;
using System.Text;
using System.Text.RegularExpressions;
using EntityFramework.Extensions;
using EntityFramework.Mapping;
using EntityFramework.Reflection;

namespace EntityFramework.Batch
{
    /// <summary>
    /// A batch execution runner for SQL Server.
    /// </summary>
    public class SqlServerBatchRunner : IBatchRunner
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
        public int Delete<TEntity>(ObjectContext objectContext, EntityMap entityMap, ObjectQuery<TEntity> query)
            where TEntity : class
        {
            DbConnection deleteConnection = null;
            DbTransaction deleteTransaction = null;
            DbCommand deleteCommand = null;
            bool ownConnection = false;
            bool ownTransaction = false;

            try
            {
                // get store connection and transaction
                var store = GetStore(objectContext);
                deleteConnection = store.Item1;
                deleteTransaction = store.Item2;

                if (deleteConnection.State != ConnectionState.Open)
                {
                    deleteConnection.Open();
                    ownConnection = true;
                }

                if (deleteTransaction == null)
                {
                    deleteTransaction = deleteConnection.BeginTransaction();
                    ownTransaction = true;
                }


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

                    sqlBuilder.AppendFormat("j0.[{0}] = j1.[{0}]", keyMap.ColumnName);
                    wroteKey = true;
                }
                sqlBuilder.Append(")");

                deleteCommand.CommandText = sqlBuilder.ToString();

                int result = deleteCommand.ExecuteNonQuery();

                // only commit if created transaction
                if (ownTransaction)
                    deleteTransaction.Commit();

                return result;
            }
            finally
            {
                if (deleteCommand != null)
                    deleteCommand.Dispose();

                if (deleteTransaction != null && ownTransaction)
                    deleteTransaction.Dispose();

                if (deleteConnection != null && ownConnection)
                    deleteConnection.Close();
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
        public int Update<TEntity>(ObjectContext objectContext, EntityMap entityMap, ObjectQuery<TEntity> query, Expression<Func<TEntity, TEntity>> updateExpression)
            where TEntity : class
        {
            DbConnection updateConnection = null;
            DbTransaction updateTransaction = null;
            DbCommand updateCommand = null;
            bool ownConnection = false;
            bool ownTransaction = false;

            try
            {
                // get store connection and transaction
                var store = GetStore(objectContext);
                updateConnection = store.Item1;
                updateTransaction = store.Item2;

                if (updateConnection.State != ConnectionState.Open)
                {
                    updateConnection.Open();
                    ownConnection = true;
                }

                // use existing transaction or create new
                if (updateTransaction == null)
                {
                    updateTransaction = updateConnection.BeginTransaction();
                    ownTransaction = true;
                }

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
                            var parameter = updateCommand.CreateParameter();
                            parameter.ParameterName = parameterName;
                            parameter.Value = value;
                            updateCommand.Parameters.Add(parameter);

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

                        value = value.Replace(alias + ".", "");

                        foreach (ObjectParameter objectParameter in selectQuery.Parameters)
                        {
                            string parameterName = "p__update__" + nameCount++;

                            var parameter = updateCommand.CreateParameter();
                            parameter.ParameterName = parameterName;
                            parameter.Value = objectParameter.Value ?? DBNull.Value;
                            updateCommand.Parameters.Add(parameter);

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

                updateCommand.CommandText = sqlBuilder.ToString();

                int result = updateCommand.ExecuteNonQuery();

                // only commit if created transaction
                if (ownTransaction)
                    updateTransaction.Commit();

                return result;
            }
            finally
            {
                if (updateCommand != null)
                    updateCommand.Dispose();
                if (updateTransaction != null && ownTransaction)
                    updateTransaction.Dispose();
                if (updateConnection != null && ownConnection)
                    updateConnection.Close();
            }
        }


        /// <summary>
        /// Create and runs a batch insert statement.
        /// </summary>
        /// <typeparam name="TEntity">The type of the entity to be inserted</typeparam>
        /// <param name="objectContext">The <see cref="ObjectContext"/> to get connection and metadata information from.</param>
        /// <param name="entityMap">The <see cref="EntityMap"/> for <typeparamref name="TEntity"/>.</param>
        /// <param name="records">Collection of <typeparamref name="TEntity"/> to be inserted</param>
        /// <returns>The number of rows inserted.</returns>
        public int BulkInsert<TEntity>(ObjectContext objectContext, EntityMap entityMap, IEnumerable<TEntity> records)
            where TEntity : class
        {
            DbConnection insertConnection = null;
            DbTransaction insertTransaction = null;
            DbCommand insertCommand = null;
            bool ownConnection = false;
            bool ownTransaction = false;

            try
            {
                // get store connection and transaction
                var store = GetStore(objectContext);
                insertConnection = store.Item1;
                insertTransaction = store.Item2;

                if (insertConnection.State != ConnectionState.Open)
                {
                    insertConnection.Open();
                    ownConnection = true;
                }

                if (insertTransaction == null)
                {
                    insertTransaction = insertConnection.BeginTransaction();
                    ownTransaction = true;
                }

                insertCommand = insertConnection.CreateCommand();
                insertCommand.Transaction = insertTransaction;

                if (objectContext.CommandTimeout.HasValue)
                    insertCommand.CommandTimeout = objectContext.CommandTimeout.Value;

                List<PropertyMap> propertyMaps = entityMap.PropertyMaps;

                var insertLine = new StringBuilder();
                insertLine.Append("INSERT INTO ");
                insertLine.AppendLine(entityMap.TableName);
                insertLine.Append(" (");
                insertLine.Append(String.Join(", ", propertyMaps
                    .Select(x => x.ColumnName)));
                insertLine.AppendLine(") VALUES ");
                var rows = new StringBuilder();
                List<DbParameter> parameters = new List<DbParameter>();
                Type entityType = records.FirstOrDefault().GetType();
                PropertyInfo[] properties = entityType.GetProperties();
                int i = 0;
                int result = 0;
                int maxRows = 2000 / propertyMaps.Count();

                foreach (var record in records)
                {
                    int propCount = parameters.Count();
                    if (propCount > 0)
                    {
                        rows.AppendLine(",");
                    }
                    rows.Append("(");
                    rows.Append(String.Join(", ", propertyMaps
                        .Select(x => String.Format("@{0}", propCount++))));
                    rows.Append(")");
                    foreach (PropertyMap propMap in propertyMaps)
                    {
                        object value = null;
                        DbParameter dbParam = insertCommand.CreateParameter();
                        dbParam.ParameterName = String.Format("@{0}", parameters.Count);
                        if (propMap is ConstantPropertyMap)
                        {
                            value = ((ConstantPropertyMap)propMap).Value;
                        }
                        else
                        {
                            PropertyInfo prop = properties.SingleOrDefault(x => x.Name == propMap.PropertyName);
                            if (prop != null)
                            {
                                value = prop.GetValue(record, new object[0]);
                                dbParam.DbType = DbTypeConversion.ToDbType(prop.PropertyType);
                            }
                        }
                        dbParam.Value = value ?? DBNull.Value;
                        parameters.Add(dbParam);
                    }
                    if (++i >= maxRows)
                    {
                        result += CommitBulk(insertCommand, insertLine, rows, parameters);
                        i = 0;
                        rows.Clear();
                        parameters.Clear();
                    }
                }
                if (rows.Length > 0)
                {
                    result += CommitBulk(insertCommand, insertLine, rows, parameters);
                }
                return result;
            }
            finally
            {
                if (insertCommand != null)
                    insertCommand.Dispose();
                if (insertTransaction != null && ownTransaction)
                    insertTransaction.Dispose();
                if (insertConnection != null && ownConnection)
                    insertConnection.Close();
            }
        }

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
        public int InsertFrom<TSource, TEntity>(ObjectContext destinationContext, EntityMap destinationEntityMap, ObjectQuery<TSource> sourceQuery, Expression<Func<TSource, TEntity>> mappingExpression)
            where TEntity : class
            where TSource : class
        {
            DbConnection insertConnection = null;
            DbTransaction insertTransaction = null;
            DbCommand insertCommand = null;
            bool ownConnection = false;
            bool ownTransaction = false;

            try
            {
                // get store connection and transaction
                var store = GetStore(destinationContext);
                insertConnection = store.Item1;
                insertTransaction = store.Item2;

                if (insertConnection.State != ConnectionState.Open)
                {
                    insertConnection.Open();
                    ownConnection = true;
                }

                if (insertTransaction == null)
                {
                    insertTransaction = insertConnection.BeginTransaction();
                    ownTransaction = true;
                }

                insertCommand = insertConnection.CreateCommand();
                insertCommand.Transaction = insertTransaction;

                if (destinationContext.CommandTimeout.HasValue)
                    insertCommand.CommandTimeout = destinationContext.CommandTimeout.Value;

                var memberInitExpression = mappingExpression.Body as MemberInitExpression;
                if (memberInitExpression == null)
                    throw new ArgumentException("The insert expression must be of type MemberInitExpression.", "insertExpression");

                var innerSelect = GetInsertSelectSql(sourceQuery, destinationEntityMap, memberInitExpression, insertCommand);
                var sqlBuilder = new StringBuilder(innerSelect.Length * 2);

                sqlBuilder.Append("INSERT INTO ");
                sqlBuilder.AppendLine(destinationEntityMap.TableName);
                sqlBuilder.Append(" (");

                sqlBuilder.Append(String.Join(", ", memberInitExpression.Bindings
                    .Select(x => destinationEntityMap.PropertyMaps
                        .Where(p => p.PropertyName == x.Member.Name)
                        .Select(p => p.ColumnName)
                        .FirstOrDefault())
                    .Union(destinationEntityMap.PropertyMaps.OfType<ConstantPropertyMap>().Select(x => x.ColumnName))));

                sqlBuilder.AppendLine(") ");
                sqlBuilder.AppendLine(innerSelect);
                insertCommand.CommandText = sqlBuilder.ToString();

                int result = insertCommand.ExecuteNonQuery();
                return result;
            }
            finally
            {
                if (insertCommand != null)
                    insertCommand.Dispose();
                if (insertTransaction != null && ownTransaction)
                    insertTransaction.Dispose();
                if (insertConnection != null && ownConnection)
                    insertConnection.Close();
            }
        }

        private static Tuple<DbConnection, DbTransaction> GetStore(ObjectContext objectContext)
        {
            DbConnection dbConnection = objectContext.Connection;
            var entityConnection = dbConnection as EntityConnection;

            // by-pass entity connection
            if (entityConnection == null)
                return new Tuple<DbConnection, DbTransaction>(dbConnection, null);

            DbConnection connection = entityConnection.StoreConnection;

            // get internal transaction
            dynamic connectionProxy = new DynamicProxy(entityConnection);
            dynamic entityTransaction = connectionProxy.CurrentTransaction;
            if (entityTransaction == null)
                return new Tuple<DbConnection, DbTransaction>(connection, null);

            DbTransaction transaction = entityTransaction.StoreTransaction;
            return new Tuple<DbConnection, DbTransaction>(connection, transaction);
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
                parameter.Value = objectParameter.Value ?? DBNull.Value;  

                command.Parameters.Add(parameter);
            }

            return innerJoinSql;
        }

        private static string GetInsertSelectSql<TSource>(ObjectQuery<TSource> query, EntityMap entityMap, MemberInitExpression memberInitExpression, DbCommand command)
            where TSource : class
        {
            var selector = new StringBuilder(50);
            int i = 0;
            var constantProperties = entityMap.PropertyMaps.OfType<ConstantPropertyMap>();
            selector.Append("new(");
            selector.Append(String.Join(", ", memberInitExpression.Bindings
                .Select(x => ((x as MemberAssignment).Expression as MemberExpression).Member.Name)
                .Union(constantProperties.Select(x => String.Format("@{0} as {1}", i++, x.PropertyName))))); //, x.PropertyName
            selector.Append(")");

            var selectQuery = DynamicQueryable.Select(query, selector.ToString(), constantProperties.Select(x => x.Value).ToArray());
            var objectQuery = selectQuery as ObjectQuery;

            if (objectQuery == null)
                throw new ArgumentException("The query must be of type ObjectQuery.", "query");
            objectQuery.EnablePlanCaching = true;
            string select = query.ToTraceString();

            // get private ObjectQueryState ObjectQuery._state;
            // of actual type internal class
            //      System.Data.Objects.ELinq.ELinqQueryState
            object queryState = GetProperty(query, "QueryState", "System.Data.Objects.ELinq.ELinqQueryState");

            // get protected ObjectQueryExecutionPlan ObjectQueryState._cachedPlan;
            // of actual type internal sealed class
            //      System.Data.Objects.Internal.ObjectQueryExecutionPlan
            object plan = GetField(queryState, "_cachedPlan", "System.Data.Objects.Internal.ObjectQueryExecutionPlan");

            // get internal readonly DbCommandDefinition ObjectQueryExecutionPlan.CommandDefinition;
            // of actual type internal sealed class
            //      System.Data.EntityClient.EntityCommandDefinition
            object commandDefinition = GetField(plan, "CommandDefinition", "System.Data.EntityClient.EntityCommandDefinition");

            // get private readonly IColumnMapGenerator EntityCommandDefinition._columnMapGenerator;
            // of actual type private sealed class
            //      System.Data.EntityClient.EntityCommandDefinition.ConstantColumnMapGenerator
            object columnMapGenerator;
            try
            {
                columnMapGenerator = GetField(commandDefinition, "_columnMapGenerator", "System.Data.EntityClient.EntityCommandDefinition+ConstantColumnMapGenerator");
            }
            catch (EFChangedException)
            {
                columnMapGenerator = GetField(commandDefinition, "_columnMapGenerators", "System.Data.EntityClient.EntityCommandDefinition+ConstantColumnMapGenerator", 0);
            }

            // get private readonly ColumnMap ConstantColumnMapGenerator._columnMap;
            // of actual type internal class
            //      System.Data.Query.InternalTrees.SimpleCollectionColumnMap
            object columnMap = GetField(columnMapGenerator, "_columnMap", "System.Data.Query.InternalTrees.SimpleCollectionColumnMap");

            // get internal ColumnMap CollectionColumnMap.Element;
            // of actual type internal class
            //      System.Data.Query.InternalTrees.RecordColumnMap
            object columnMapElement = GetProperty(columnMap, "Element", "System.Data.Query.InternalTrees.RecordColumnMap");

            // get internal ColumnMap[] StructuredColumnMap.Properties;
            // array of internal abstract class
            //      System.Data.Query.InternalTrees.ColumnMap
            Array columnMapProperties = GetProperty(columnMapElement, "Properties", "System.Data.Query.InternalTrees.ColumnMap[]") as Array;

            int n = columnMapProperties.Length;
            var cols = select.Substring(select.IndexOf("SELECT") + 6, select.IndexOf("FROM") - 6)
                        .Split(new char[] { ',' }, columnMapProperties.Length + 1);
            StringBuilder innerSelectBuilder = new StringBuilder(select.Length * 2);
            for (int j = 0; j < n; ++j)
            {
                // get value at index j in array
                // of actual type internal class
                //      System.Data.Query.InternalTrees.ScalarColumnMap
                object column = columnMapProperties.GetValue(j);
                if (column == null || column.GetType().FullName != "System.Data.Query.InternalTrees.ScalarColumnMap") throw new EFChangedException();

                //string colName = (string)GetProp(column, "Name");
                // can be used for more advanced bingings

                // get internal int ScalarColumnMap.ColumnPos;
                object columnPositionOfAProperty = GetProperty(column, "ColumnPos", "System.Int32");

                if (innerSelectBuilder.Length > 0)
                {
                    innerSelectBuilder.Append(", ");
                }
                innerSelectBuilder.Append(cols[(int)columnPositionOfAProperty]);
            }
            innerSelectBuilder.Insert(0, "SELECT ");
            innerSelectBuilder.Append(select.Substring(select.IndexOf("FROM")));
            string innserSelectSql = innerSelectBuilder.ToString();
            // create parameters
            foreach (var objectParameter in objectQuery.Parameters)
            {
                var parameter = command.CreateParameter();
                parameter.ParameterName = objectParameter.Name;
                parameter.Value = objectParameter.Value ?? DBNull.Value;

                command.Parameters.Add(parameter);
            }

            return innserSelectSql;
        }

        internal class EFChangedException : InvalidOperationException
        {
            internal EFChangedException() : base("Entity Framework internals have changed") { }
        }

        static object GetProperty(object obj, string propName, string expectedType)
        {
            PropertyInfo prop = obj.GetType().GetProperty(propName, BindingFlags.NonPublic | BindingFlags.Instance);
            if (prop == null) throw new EFChangedException();
            object value = prop.GetValue(obj, new object[0]);
            if (value == null || value.GetType().FullName != expectedType) throw new EFChangedException();
            return value;
        }

        static object GetField(object obj, string fieldName, string expectedType, int index = -1)
        {
            FieldInfo field = obj.GetType().GetField(fieldName, BindingFlags.NonPublic | BindingFlags.Instance);
            if (field == null) throw new EFChangedException();
            var result = field.GetValue(obj);
            if (index >= 0 && result is Array)
            {
                result = ((Array)result).GetValue(index);
            }
            if (result == null || result.GetType().FullName != expectedType) throw new EFChangedException();
            return result;
        }

        private static int CommitBulk(DbCommand command, StringBuilder insertLine, StringBuilder rows, List<DbParameter> parameters)
        {
            string commandText = insertLine.ToString() + rows.ToString();
            int parameterCount = 0;
            if (command.CommandText != commandText)
            {
                command.CommandText = commandText;
                command.Parameters.Clear();
                command.Parameters.AddRange(parameters.ToArray());
            }
            else
            {
                parameters.ForEach(x => command.Parameters[parameterCount++].Value = x.Value);
            }
            return command.ExecuteNonQuery();
        }
    }

    /// <summary>
    /// Convert a base data type to another base data type
    /// </summary>
    public static class DbTypeConversion
    {
        private class DbTypeMapEntry
        {
            public Type Type;
            public DbType DbType;
            public SqlDbType SqlDbType;
            public DbTypeMapEntry(Type type, DbType dbType, SqlDbType sqlDbType)
            {
                this.Type = type;
                this.DbType = dbType;
                this.SqlDbType = sqlDbType;
            }
        };
        private static List<DbTypeMapEntry> dbTypeList = new List<DbTypeMapEntry>();

        #region Constructors
        static DbTypeConversion()
        {
            dbTypeList.Add(new DbTypeMapEntry(typeof(bool), DbType.Boolean, SqlDbType.Bit));
            dbTypeList.Add(new DbTypeMapEntry(typeof(byte), DbType.Double, SqlDbType.TinyInt));
            dbTypeList.Add(new DbTypeMapEntry(typeof(byte[]), DbType.Binary, SqlDbType.Image));
            dbTypeList.Add(new DbTypeMapEntry(typeof(DateTime), DbType.DateTime, SqlDbType.DateTime));
            dbTypeList.Add(new DbTypeMapEntry(typeof(Decimal), DbType.Decimal, SqlDbType.Decimal));
            dbTypeList.Add(new DbTypeMapEntry(typeof(double), DbType.Double, SqlDbType.Float));
            dbTypeList.Add(new DbTypeMapEntry(typeof(Guid), DbType.Guid, SqlDbType.UniqueIdentifier));
            dbTypeList.Add(new DbTypeMapEntry(typeof(Int16), DbType.Int16, SqlDbType.SmallInt));
            dbTypeList.Add(new DbTypeMapEntry(typeof(Int32), DbType.Int32, SqlDbType.Int));
            dbTypeList.Add(new DbTypeMapEntry(typeof(Int64), DbType.Int64, SqlDbType.BigInt));
            dbTypeList.Add(new DbTypeMapEntry(typeof(object), DbType.Object, SqlDbType.Variant));
            dbTypeList.Add(new DbTypeMapEntry(typeof(string), DbType.String, SqlDbType.VarChar));
            dbTypeList.Add(new DbTypeMapEntry(typeof(DBNull), DbType.Object, SqlDbType.Variant));
        }

        #endregion

        #region Methods
        /// <summary>
        /// Convert db type to .Net data type
        /// </summary>
        /// <param name="dbType"></param>
        /// <returns></returns>
        public static Type ToNetType(DbType dbType)
        {
            DbTypeMapEntry entry = Find(dbType);
            return entry.Type;
        }

        /// <summary>
        /// Convert TSQL type to .Net data type
        /// </summary>
        /// <param name="sqlDbType"></param>
        /// <returns></returns>
        public static Type ToNetType(SqlDbType sqlDbType)
        {
            DbTypeMapEntry entry = Find(sqlDbType);
            return entry.Type;
        }
        /// <summary>
        /// Convert .Net type to Db type
        /// </summary>
        /// <param name="type"></param>
        /// <returns></returns>
        public static DbType ToDbType(Type type)
        {
            DbTypeMapEntry entry = Find(type);
            return entry.DbType;
        }
        /// <summary>
        /// Convert TSQL data type to DbType
        /// </summary>
        /// <param name="sqlDbType"></param>
        /// <returns></returns>
        public static DbType ToDbType(SqlDbType sqlDbType)
        {
            DbTypeMapEntry entry = Find(sqlDbType);
            return entry.DbType;
        }

        /// <summary>
        /// Convert .Net type to TSQL data type
        /// </summary>
        /// <param name="type"></param>
        /// <returns></returns>
        public static SqlDbType ToSqlDbType(Type type)
        {
            DbTypeMapEntry entry = Find(type);
            return entry.SqlDbType;
        }

        /// <summary>
        /// Convert DbType type to TSQL data type
        /// </summary>
        /// <param name="dbType"></param>
        /// <returns></returns>
        public static SqlDbType ToSqlDbType(DbType dbType)
        {
            DbTypeMapEntry entry = Find(dbType);
            return entry.SqlDbType;
        }

        private static DbTypeMapEntry Find(Type type)
        {
            if (type.IsGenericType && type.GetGenericTypeDefinition() == typeof(Nullable<>))
            {
                type = type.GetGenericArguments()[0];
            }
            DbTypeMapEntry result = dbTypeList.FirstOrDefault(x => x.Type == type);
            if (result == null)
            {
                throw new ApplicationException("Referenced an unsupported Type");
            }
            return result;
        }
        private static DbTypeMapEntry Find(DbType dbType)
        {
            DbTypeMapEntry result = dbTypeList.FirstOrDefault(x => x.DbType == dbType);
            if (result == null)
            {
                throw new ApplicationException("Referenced an unsupported DbType");
            }
            return result;

        }
        private static DbTypeMapEntry Find(SqlDbType sqlDbType)
        {
            DbTypeMapEntry result = dbTypeList.FirstOrDefault(x => x.SqlDbType == sqlDbType);
            if (result == null)
            {
                throw new ApplicationException("Referenced an unsupported SqlDbType");
            }
            return result;
        }
        #endregion
    }
}
