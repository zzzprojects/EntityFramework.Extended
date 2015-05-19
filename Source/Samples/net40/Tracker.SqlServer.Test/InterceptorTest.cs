using System;
using System.Collections.Generic;
using System.Data.Entity.Infrastructure.Interception;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using NSubstitute;
using Tracker.SqlServer.CodeFirst;
using Xunit;
using EntityFramework.Extensions;
using EntityFramework.Future;
using System.Data.Common;

namespace Tracker.SqlServer.Test
{
    /// <summary>
    /// Tests interceptors with future queries
    /// </summary>
    public class InterceptorTest
    {
        [Fact]
        public void TestInterception()
        {
            using(var db = new TrackerContext())
            {
                var interceptor = Substitute.For<IDbCommandInterceptor>();
                
                DbInterception.Add(interceptor);

                // run a future query
                var q = db.Tasks.Where(p => p.PriorityId == 2)
                    .OrderByDescending(t => t.CreatedDate);
                
                var q1 = q.FutureCount();                
                var q2 = q.Skip(0).Take(10).Future();
                
                var tasks = q2.ToList();
                int total = q1.Value;

                // assert that all the appropriate interceptor methods were called
                interceptor.Received().ReaderExecuting(
                    Arg.Any<DbCommand>(), 
                    Arg.Is<DbCommandInterceptionContext<DbDataReader>>(
                        ctx => ctx.DbContexts != null                            
                            && ctx.DbContexts.Count(x => x is TrackerContext) == 1));

                interceptor.Received().ReaderExecuted(
                    Arg.Any<DbCommand>(), 
                    Arg.Is<DbCommandInterceptionContext<DbDataReader>>(
                        ctx => ctx.DbContexts != null
                            && ctx.DbContexts.Count(x => x is TrackerContext) == 1));

                interceptor.Received().ScalarExecuting(
                  Arg.Any<DbCommand>(),
                  Arg.Any<DbCommandInterceptionContext<object>>());

                interceptor.Received().ScalarExecuted(
                    Arg.Any<DbCommand>(),
                    Arg.Any<DbCommandInterceptionContext<object>>());

                interceptor.DidNotReceive().NonQueryExecuting(
                    Arg.Any<DbCommand>(),
                    Arg.Any<DbCommandInterceptionContext<int>>());

                interceptor.DidNotReceive().NonQueryExecuted(
                    Arg.Any<DbCommand>(),
                    Arg.Any<DbCommandInterceptionContext<int>>());
                              
                DbInterception.Remove(interceptor);
            }
        }
    }
}
