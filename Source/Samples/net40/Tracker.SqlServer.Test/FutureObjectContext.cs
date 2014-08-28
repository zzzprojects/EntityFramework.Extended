using System;
using System.Text;
using System.Collections.Generic;
using System.Linq;
using EntityFramework.Extensions;
using EntityFramework.Future;
using Xunit;
using Tracker.SqlServer.Entities;

namespace Tracker.SqlServer.Test
{
    
    public class FutureObjectContext
    {

        [Fact]
        public void PageTest()
        {
            var db = new TrackerEntities();

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
            var db = new TrackerEntities();

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
            var db = new TrackerEntities();

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
            var db = new TrackerEntities();

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
            var db = new TrackerEntities();

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
            var db = new TrackerEntities();
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

    }
}
