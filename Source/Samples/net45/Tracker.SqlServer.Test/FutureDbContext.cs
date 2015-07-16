using System.Linq;
using System.Text;
using EntityFramework.Extensions;
using EntityFramework.Future;
using Xunit;
using Tracker.SqlServer.CodeFirst;
using Tracker.SqlServer.CodeFirst.Entities;

namespace Tracker.SqlServer.Test
{
    public class FutureDbContext
    {
        [Fact]
        public void PageTest()
        {
            var db = new TrackerContext();

            // base query
            var q = db.Tasks
                .Where(p => p.PriorityId == 2)
                .OrderByDescending(t => t.CreatedDate);

            // get total count
            var q1 = q.FutureCount();
            // get first page
            var q2 = q.Skip(0).Take(10).Future();
            // triggers sql execute as a batch
            var tasks = q2.ToList();
            int total = q1.Value;

            Assert.NotNull(tasks);
        }

        [Fact]
        public void SimpleTest()
        {
            var db = new TrackerContext();

            // build up queries

            string emailDomain = "@battlestar.com";
            var q1 = db.Users
                .Where(p => p.EmailAddress.EndsWith(emailDomain))
                .Future();

            string search = "Earth";
            var q2 = db.Tasks
                .Where(t => t.Summary.Contains(search))
                .Future();

            // should be 2 queries 
            //Assert.Equal(2, db.FutureQueries.Count);

            // this triggers the loading of all the future queries
            var users = q1.ToList();
            Assert.NotNull(users);

            // should be cleared at this point
            //Assert.Equal(0, db.FutureQueries.Count);

            // this should already be loaded
            Assert.True(((IFutureQuery)q2).IsLoaded);

            var tasks = q2.ToList();
            Assert.NotNull(tasks);

        }

        [Fact]
        public void FutureCountTest()
        {
            var db = new TrackerContext();

            // build up queries

            string emailDomain = "@battlestar.com";
            var q1 = db.Users
                .Where(p => p.EmailAddress.EndsWith(emailDomain))
                .Future();

            string search = "Earth";
            var q2 = db.Tasks
                .Where(t => t.Summary.Contains(search))
                .FutureCount();

            // should be 2 queries 
            //Assert.Equal(2, db.FutureQueries.Count);

            // this triggers the loading of all the future queries
            var users = q1.ToList();
            Assert.NotNull(users);

            // should be cleared at this point
            //Assert.Equal(0, db.FutureQueries.Count);

            // this should already be loaded
            Assert.True(((IFutureQuery)q2).IsLoaded);

            int count = q2;
            Assert.NotEqual(count, 0);
        }

        [Fact]
        public void FutureCountReverseTest()
        {
            var db = new TrackerContext();

            // build up queries

            string emailDomain = "@battlestar.com";
            var q1 = db.Users
                .Where(p => p.EmailAddress.EndsWith(emailDomain))
                .Future();

            string search = "Earth";
            var q2 = db.Tasks
                .Where(t => t.Summary.Contains(search))
                .FutureCount();

            // should be 2 queries 
            //Assert.Equal(2, db.FutureQueries.Count);

            // access q2 first to trigger loading, testing loading from FutureCount
            // this triggers the loading of all the future queries
            var count = q2.Value;
            Assert.NotEqual(count, 0);

            // should be cleared at this point
            //Assert.Equal(0, db.FutureQueries.Count);

            // this should already be loaded
            Assert.True(((IFutureQuery)q1).IsLoaded);

            var users = q1.ToList();
            Assert.NotNull(users);
        }

        [Fact]
        public void FutureValueTest()
        {
            var db = new TrackerContext();

            // build up queries
            string emailDomain = "@battlestar.com";
            var q1 = db.Users
                .Where(p => p.EmailAddress.EndsWith(emailDomain))
                .FutureFirstOrDefault();

            string search = "Earth";
            var q2 = db.Tasks
                .Where(t => t.Summary.Contains(search))
                .FutureCount();

            // duplicate query except count
            var q3 = db.Tasks
                .Where(t => t.Summary.Contains(search))
                .Future();

            // should be 3 queries 
            //Assert.Equal(3, db.FutureQueries.Count);

            // this triggers the loading of all the future queries
            User user = q1;
            Assert.NotNull(user);

            // should be cleared at this point
            //Assert.Equal(0, db.FutureQueries.Count);

            // this should already be loaded
            Assert.True(((IFutureQuery)q2).IsLoaded);

            var count = q2.Value;
            Assert.NotEqual(count, 0);

            var tasks = q3.ToList();
            Assert.NotNull(tasks);
        }

        [Fact]
        public void FutureValueReverseTest()
        {
            var db = new TrackerContext();
            // build up queries

            string emailDomain = "@battlestar.com";
            var q1 = db.Users
                .Where(p => p.EmailAddress.EndsWith(emailDomain))
                .FutureFirstOrDefault();

            string search = "Earth";
            var q2 = db.Tasks
                .Where(t => t.Summary.Contains(search))
                .FutureCount();

            // duplicate query except count
            var q3 = db.Tasks
                .Where(t => t.Summary.Contains(search))
                .Future();

            // should be 3 queries 
            //Assert.Equal(3, db.FutureQueries.Count);

            // access q2 first to trigger loading, testing loading from FutureCount
            // this triggers the loading of all the future queries
            var count = q2.Value;
            Assert.NotEqual(count, 0);

            // should be cleared at this point
            //Assert.Equal(0, db.FutureQueries.Count);

            // this should already be loaded
            Assert.True(((IFutureQuery)q1).IsLoaded);

            var users = q1.Value;
            Assert.NotNull(users);

            var tasks = q3.ToList();
            Assert.NotNull(tasks);

        }

        [Fact]
        public void FutureValueWithAggregateFunctions()
        {
            var db = new TrackerContext();

            var q1 = db.Users.Where(x => x.EmailAddress.EndsWith("@battlestar.com")).FutureValue(x => x.Count());
            var q2 = db.Users.Where(x => x.EmailAddress.EndsWith("@battlestar.com")).FutureValue(x => x.Min(t => t.LastName));
            var q3 = db.Tasks.FutureValue(x => x.Sum(t => t.Priority.Order));

            Assert.False(((IFutureQuery)q1).IsLoaded);
            Assert.False(((IFutureQuery)q2).IsLoaded);
            Assert.False(((IFutureQuery)q3).IsLoaded);

            var r1 = q1.Value;
            var r2 = q2.Value;
            var r3 = q3.Value;

            Assert.True(((IFutureQuery)q1).IsLoaded);
            Assert.True(((IFutureQuery)q2).IsLoaded);
            Assert.True(((IFutureQuery)q3).IsLoaded);
        }

        [Fact]
        public void LoggingInterception()
        {
            //log to a string builder
            var sb = new StringBuilder();

            var db = new TrackerContext();
            db.Database.Log = s => sb.AppendLine(s);

            const string emailDomain = "@battlestar.com";
            var q1 = db.Users
                .Where(p => p.EmailAddress.EndsWith(emailDomain))
                .FutureFirstOrDefault();

            //materialize it
            var user = q1.Value;

            //did we log anything?
            var logged = sb.ToString();

            Assert.Contains("[EmailAddress] LIKE N'%@battlestar.com'", logged);
        }
    }
}