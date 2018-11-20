using EntityFramework.Extensions;
using EntityFramework.Mapping;
using EntityFramework.Reflection;
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
using System.Threading.Tasks;

namespace EntityFramework.Batch
{
    internal static class QueryHelper
    {
        public class Db : IDisposable
        {
            public Db(ObjectContext context)
            {
                _loggers = context.InterceptionContext.DbContexts.Select(ctx => ctx.Database.Log).Where(log => log != null).ToArray();
            }

            public DbConnection Connection;
            public DbTransaction Transaction;
            public DbCommand Command;
            public bool OwnConnection;
            public bool OwnTransaction;

            private Action<string>[] _loggers;

            public void Log(string log)
            {
                foreach (var logger in _loggers) logger(log);
            }

            public void Dispose()
            {
                if (Command != null)
                    Command.Dispose();
                if (Transaction != null && OwnTransaction)
                    Transaction.Dispose();
                if (Connection != null && OwnConnection)
                    Connection.Close();
            }
        }

        public static Tuple<DbConnection, DbTransaction> GetStore(ObjectContext objectContext)
        {
            // TODO, re-eval if this is needed

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

        public static Db GetDb(ObjectContext context)
        {
            var db = new Db(context);
            try
            {
                var store = GetStore(context);
                db.Connection = store.Item1;
                db.Transaction = store.Item2;

                if (db.Connection.State != ConnectionState.Open)
                {
                    db.Connection.Open();
                    db.OwnConnection = true;
                }

                // use existing transaction or create new
                if (db.Transaction == null)
                {
                    db.Transaction = db.Connection.BeginTransaction();
                    db.OwnTransaction = true;
                }

                db.Command = db.Connection.CreateCommand();
                db.Command.Transaction = db.Transaction;
                if (context.CommandTimeout.HasValue)
                    db.Command.CommandTimeout = context.CommandTimeout.Value;
            } catch (Exception ex) {
                db.Dispose();
                throw ex;
            }
            return db;
        }

        public static void CopyTo(this ObjectParameterCollection parameters, DbCommand command, Func<string, string> fnGetParamName = null, IBatchRunner runner = null)
        {
            fnGetParamName = fnGetParamName ?? (paramName => paramName);
            runner = runner ?? BatchExtensions.ResolveRunner();
            var nullValue = runner.DbNull;
            foreach (var objectParameter in parameters)
            {
                var parameter = command.CreateParameter();
                parameter.ParameterName = fnGetParamName(objectParameter.Name);
                parameter.Value = objectParameter.Value ?? nullValue;
                command.Parameters.Add(parameter);
            }
        }

        public static IList<string> GetPropertyPositions(IQueryable query, IList<string> fields = null)
        {
            if (fields == null) fields = new List<string>();
            fields.Clear();
            var objectQuery = query as ObjectQuery;
            var maps = new NonPublicMember(objectQuery).GetProperty("QueryState").GetField("_cachedPlan").GetField("CommandDefinition")
                    .GetField("_columnMapGenerators").Idx(0).GetField("_columnMap").GetProperty("Element").GetProperty("Properties").Value as Array;
            foreach (var m in maps)
            {
                var mm = new NonPublicMember(m);
                int pos = (int)mm.GetProperty("ColumnPos").Value;
                string name = (string)mm.GetProperty("Name").Value;
                while (pos + 1 > fields.Count) fields.Add(null); //advance the size of [fields] if the pos index is not covered by it
                fields[pos] = name;
            }
            return fields;
        }

        public static string GetSelectSql<TModel>(ObjectQuery<TModel> query, IEnumerable<string> selectedProperties, DbCommand command = null,
            IBatchRunner runner = null, IList<string> fields = null)
        {
            if (selectedProperties == null || selectedProperties.Count() < 1)
                throw new ArgumentException("The selected properties must be defined.", "selectedProperties");

            var selector = new StringBuilder(string.Join(",", selectedProperties)).Insert(0, "new (").Append(")");
            var selectQuery = DynamicQueryable.Select(query, selector.ToString());
            var objectQuery = selectQuery as ObjectQuery;

            if (objectQuery == null)
                throw new ArgumentException("The query must be of type ObjectQuery.", "query");

            string selectSql = objectQuery.ToTraceString();
            if (command != null) objectQuery.Parameters.CopyTo(command, runner: runner);

            if (fields != null)
            {
                GetPropertyPositions(selectQuery, fields);
            }

            return selectSql;
        }

        public static string GetSelectKeySql<TEntity>(ObjectQuery<TEntity> query, EntityMap entityMap, DbCommand command, IBatchRunner runner, IList<string> fields = null)
            where TEntity : class
        {
            // TODO change to esql?

            // changing query to only select keys
            var idFields = entityMap.KeyMaps.Select(key => key.PropertyName);
            string sql = GetSelectSql(query, idFields, command, runner, fields);
            //if (fields != null)
            //{
            //    for (int i = 0; i < fields.Count; i++) if (!idFields.Contains(fields[i])) fields[i] = null;
            //}
            return sql;
        }

        public static IEnumerable<string> GetSelectedProperties(Expression queryExpression, EntityMap entityMap)
        {
            var methodExp = queryExpression as MethodCallExpression;
            if (methodExp == null) return null;
            if (methodExp.Method.Name != "Select" && methodExp.Method.Name != "SelectMany" //From Select method we know the selected properties
                && methodExp.Method.Name != "Join" && methodExp.Method.Name != "GroupBy" //It can also from Join or GroupBy method
                && methodExp.Method.Name != "MergeAs" /*MergeAs, if ends up at a dbContext.DbSet, without Select method */
            )
                return GetSelectedProperties(methodExp.Arguments[0], entityMap);

            Func<IEnumerable<string>> getProperties = () =>
            {
                return from property in entityMap.ModelType.Properties
                       select property.Name;
            };

            if (methodExp.Method.Name == "MergeAs")
                return getProperties();

            var parameterExp = methodExp.Arguments.Last() as UnaryExpression;
            if (parameterExp == null) return null;
            var selectorExp = parameterExp.Operand as LambdaExpression;
            if (selectorExp == null) return null;
            if (selectorExp.Body is ParameterExpression)
            {
                return getProperties();
            }
            else if (selectorExp.Body is MemberInitExpression)
            {
                return from memberBinding in (selectorExp.Body as MemberInitExpression).Bindings
                       select memberBinding.Member.Name;
            }
            return null;
        }

#if NET45
        internal static async Task<int> InternalInsert<TModel>(this IBatchRunner runner, IQueryable<TModel> query, ObjectQuery<TModel> objectQuery,
            EntityMap entityMap, bool async = false)
            where TModel : class
#else
        internal static int InternalInsert<TModel>(this IBatchRunner runner, IQueryable<TModel> query, ObjectQuery<TModel> objectQuery,
            EntityMap entityMap, bool async = false)
            where TModel : class
#endif
        {
            using (var db = QueryHelper.GetDb(objectQuery.Context))
            {
                var selectedProperties = QueryHelper.GetSelectedProperties(query.Expression, entityMap);
                if (selectedProperties == null)
                    throw new ArgumentException("Cannot read the selected fields in the query", "sourceQuery");

                var insertFields = new List<string>();
                var selectSql = GetSelectSql(objectQuery, selectedProperties, db.Command, runner, insertFields);
                bool isThereUnusedField = false;
                foreach (var f in insertFields) if (f == null) { isThereUnusedField = true;  break; }
                var selectFields = isThereUnusedField ? new SelectedFields(selectSql, runner.CharToEscapeQuote) : null;
                var sqlBuilder = new StringBuilder(selectSql.Length * 2);
                sqlBuilder.Append("INSERT INTO ").Append(entityMap.TableName).Append(" (")
                    .Append(string.Join(", ",
                            from propName in insertFields
                            join map in entityMap.PropertyMaps on propName equals map.PropertyName
                            where propName != null
                            select runner.Quote(map.ColumnName)
                    ))
                    .Append(")")
                    .Append(Environment.NewLine);
                if (isThereUnusedField)
                {
                    sqlBuilder.Append("SELECT");
                    string separator = " ";
                    for (int i = 0; i < insertFields.Count; i++)
                    {
                        if (insertFields[i] != null)
                        {
                            sqlBuilder.Append(separator).Append(selectFields[i]);
                            separator = ", ";
                        }
                    }
                    sqlBuilder.Append(Environment.NewLine);
                    sqlBuilder.Append(selectSql.Substring(selectFields.FromIndex));
                }
                else
                    sqlBuilder.Append(selectSql);

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

        internal static Tuple<Db, string, SelectedFields, Dictionary<string, string>, string, SelectedFields, List<string>> UpdateJoinParam<TModel>(this IBatchRunner runner,
            IQueryable<TModel> query, ObjectQuery<TModel> objectQuery, EntityMap entityMap)
            where TModel : class
        {
            var db = GetDb(objectQuery.Context);
            string keySelect = null;
            int i = 0;
            var idFields = new List<string>();
            try
            {
                keySelect = GetSelectKeySql(objectQuery, entityMap, null, runner, idFields);
            }
            catch (NotSupportedException ex)
            {
                //if (ex.HResult == -2146233067)
                    throw new ArgumentException("The select statement must include the key(s) of updated table. The keys themselves would not be updated.", "query");
                //throw ex;
            }
            var selectKeyFields = new SelectedFields(keySelect, runner.CharToEscapeQuote);
            var idFieldsMap = new Dictionary<string, string>();
            for (i = 0; i < idFields.Count; i++)
            {
                if (idFields[i] == null) continue;
                string valueField = selectKeyFields[i];
                if (selectKeyFields.AliasIndexes[i] > 0) valueField = valueField.Substring(0, selectKeyFields.AliasIndexes[i]);
                idFieldsMap[idFields[i]] = valueField;
            }

            var selectedProperties = GetSelectedProperties(query.Expression, entityMap);
            if (selectedProperties == null || selectedProperties.Count() < 1)
                throw new ArgumentException("Cannot read the selected fields in the query", "query");

            var updateFields = new List<string>();
            string selectSql = GetSelectSql(objectQuery, selectedProperties, db.Command, runner, updateFields);
            var selectFields = new SelectedFields(selectSql, runner.CharToEscapeQuote);

            return Tuple.Create(db, keySelect, selectKeyFields, idFieldsMap, selectSql, selectFields, updateFields);
        }

        public class SelectedFields : List<string>
        {
            public SelectedFields(string sql, char escapingQuoteChar)
            {
                if (!sql.StartsWith("SELECT", StringComparison.OrdinalIgnoreCase)) return;
                bool isEnd = false, isInStr = false;
                int beginAlias = 0;
                List<int> aliasIdxs = new List<int>();
                var field = new List<char>();
                int idx = "SELECT".Length;
                while (idx < sql.Length)
                {
                    if (field.Count < 1 /*The beginning of a field section*/) while (char.IsWhiteSpace(sql[idx])) idx++;
                    char ch = sql[idx];
                    if (isInStr)
                    {
                        field.Add(ch);
                        int idx2 = idx + 1;
                        if (ch == escapingQuoteChar && idx2 < sql.Length && sql[idx2] == '\'')
                        {
                            field.Add(sql[++idx]); //Don't consider this quote character as the end of string
                        }
                        else if (ch == '\'') //The end of string
                        {
                            isInStr = false;
                        }
                    }
                    else
                    {
                        if (ch == ',')
                        {
                            this.Add(new string(field.ToArray()).TrimEnd());
                            aliasIdxs.Add(beginAlias);
                            field.Clear();
                            beginAlias = 0;
                        }
                        else
                        {
                            field.Add(ch);
                            if (ch == '\'') //The beginning of string. In the string, "FROM" and ',' will not be interpreted as a boundary
                                isInStr = true;
                            else if (field.Count >= 6 && char.IsWhiteSpace(field[field.Count-6])
                                && "FROM".Equals(new string(field.GetRange(field.Count-5, 4).ToArray()), StringComparison.OrdinalIgnoreCase)
                                && char.IsWhiteSpace(field.Last())) //The beginning of FROM clause (the end of SELECT clause)
                            {
                                isEnd = true;
                                FromIndex = idx - 4;
                                this.Add(new string(field.GetRange(0, field.Count - 6).ToArray()).TrimEnd());
                                aliasIdxs.Add(beginAlias);
                                break;
                            }
                            else if (field.Count > 4 && char.IsWhiteSpace(field[field.Count - 4])
                                && char.ToUpper(field[field.Count - 3]) == 'A' && char.ToUpper(field[field.Count - 2]) == 'S'
                                && char.IsWhiteSpace(field.Last())) //The beginning of FROM clause (the end of SELECT clause)
                            {
                                beginAlias = field.Count - 4;
                            }
                        }
                    }
                    idx++;
                }
                if (!isEnd) this.Clear();
                else AliasIndexes = aliasIdxs.ToArray();
            }

            public int FromIndex { get; private set; }
            public int[] AliasIndexes { get; private set; }
        }
    }


    public class NonPublicMember
    {
        private object _obj;
        public NonPublicMember(object obj)
        {
            if (obj == null) throw new ArgumentNullException("obj");
            _obj = obj;
        }

        public NonPublicMember GetProperty(string name)
        {
            var p = GetProperty(_obj, name);
            if (p == null) throw new Exception("No property named " + name + " in type " + _obj.GetType().FullName);
            object value = p.GetValue(_obj, null);
            return new NonPublicMember(value);
        }

        public static PropertyInfo GetProperty(object obj, string name)
        {
            return obj.GetType().GetProperty(name, BindingFlags.NonPublic | BindingFlags.Instance);
        }

        public NonPublicMember GetField(string name)
        {
            var f = GetField(_obj, name);
            if (f == null) throw new Exception("No field named " + name + " in type " + _obj.GetType().FullName);
            object value = f.GetValue(_obj);
            return new NonPublicMember(value);
        }

        public static FieldInfo GetField(object obj, string name)
        {
            return obj.GetType().GetField(name, BindingFlags.NonPublic | BindingFlags.Instance);
        }

        public NonPublicMember Idx(int idx)
        {
            var objs = _obj as Array;
            if (objs == null) throw new Exception(_obj?.GetType().FullName + " not an array");
            return new NonPublicMember(objs.GetValue(idx));
        }

        public object Value { get { return _obj; } }
    }
}
