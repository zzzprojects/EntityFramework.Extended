using System;
using System.Collections.Generic;
using System.Data.Common;
using System.Data.Entity.Core.EntityClient;
using System.Data.Entity.Core.Objects;
using System.Data.Entity.Infrastructure.Interception;
using System.Text;
using EntityFramework.Reflection;

namespace EntityFramework.Future
{
    /// <summary>
    /// A class encapsulating the execution of future queries.
    /// </summary>
    public class FutureRunner : IFutureRunner
    {
        /// <summary>
        /// Executes the future queries.
        /// </summary>
        /// <param name="context">The <see cref="ObjectContext"/> to run the queries against.</param>
        /// <param name="futureQueries">The future queries list.</param>
        public void ExecuteFutureQueries(ObjectContext context, IList<IFutureQuery> futureQueries)
        {
            if (context == null)
                throw new ArgumentNullException("context");
            if (futureQueries == null)
                throw new ArgumentNullException("futureQueries");

            // used to call internal methods
            dynamic contextProxy = new DynamicProxy(context);
            contextProxy.EnsureConnection(false);

            //the (internal) InterceptionContext contains the registered loggers
            DbInterceptionContext interceptionContext = contextProxy.InterceptionContext;

            try
            {
                using (var command = CreateFutureCommand(context, futureQueries))
                using (var reader = DbInterception.Dispatch.Command.Reader(command, new DbCommandInterceptionContext(interceptionContext)))
                {
                    foreach (var futureQuery in futureQueries)
                    {
                        futureQuery.SetResult(context, reader);
                        reader.NextResult();
                    }
                }
            }
            finally
            {
                contextProxy.ReleaseConnection();
                // once all queries processed, clear from queue
                futureQueries.Clear();
            }
        }

        private static DbCommand CreateFutureCommand(ObjectContext context, IEnumerable<IFutureQuery> futureQueries)
        {
            DbConnection dbConnection = context.Connection;
            var entityConnection = dbConnection as EntityConnection;

            // by-pass entity connection, doesn't support multiple results.
            var command = entityConnection == null
                              ? dbConnection.CreateCommand()
                              : entityConnection.StoreConnection.CreateCommand();

            if (entityConnection != null && entityConnection.CurrentTransaction != null)
                {
                command.Transaction = entityConnection.CurrentTransaction.StoreTransaction;
                }

            var futureSql = new StringBuilder();
            int queryCount = 0;

            foreach (IFutureQuery futureQuery in futureQueries)
            {
                var plan = futureQuery.GetPlan(context);
                string sql = plan.CommandText;

                // clean up params
                foreach (var parameter in plan.Parameters)
                {
                    string orginal = parameter.Name;
                    string updated = string.Format("f{0}_{1}", queryCount, orginal);

                    sql = sql.Replace("@" + orginal, "@" + updated);

                    var dbParameter = command.CreateParameter();
                    dbParameter.ParameterName = updated;
                    if (parameter.Value == null)
                    {
                        dbParameter.Value = DBNull.Value;
                    }
                    else
                    {
                        dbParameter.Value = parameter.Value;
                    }

                    command.Parameters.Add(dbParameter);
                }

                // add sql
                if (futureSql.Length > 0)
                    futureSql.AppendLine();

                futureSql.Append("-- Query #");
                futureSql.Append(queryCount + 1);
                futureSql.AppendLine();
                futureSql.AppendLine();

                futureSql.Append(sql.Trim());
                futureSql.AppendLine(";");

                queryCount++;
            } // foreach query

            command.CommandText = futureSql.ToString();
            if (context.CommandTimeout.HasValue)
                command.CommandTimeout = context.CommandTimeout.Value;

            return command;
        }
    }
}