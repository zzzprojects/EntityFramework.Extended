using System;
using System.Text;
using System.Collections.Generic;
using System.Linq;
using EntityFramework.Extensions;
using EntityFramework.Future;
using NUnit.Framework;
using Tracker.SqlServer.Entities;

namespace Tracker.SqlServer.Test
{
    [TestFixture]
    public class FutureObjectContext
    {

        [Test]
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

            Assert.IsNotNull(tasks);
        }

        [Test]
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
            //Assert.AreEqual(2, db.FutureQueries.Count);

            // this triggers the loading of all the future queries
            var users = q1.ToList();
            Assert.IsNotNull(users);

            // should be cleared at this point
            //Assert.AreEqual(0, db.FutureQueries.Count);

            // this should already be loaded
            Assert.IsTrue(((IFutureQuery)q2).IsLoaded);

            var tasks = q2.ToList();
            Assert.IsNotNull(tasks);

        }

        [Test]
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
            //Assert.AreEqual(2, db.FutureQueries.Count);

            // this triggers the loading of all the future queries
            var users = q1.ToList();
            Assert.IsNotNull(users);

            // should be cleared at this point
            //Assert.AreEqual(0, db.FutureQueries.Count);

            // this should already be loaded
            Assert.IsTrue(((IFutureQuery)q2).IsLoaded);

            int count = q2;
            Assert.AreNotEqual(count, 0);
        }

        [Test]
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
            //Assert.AreEqual(2, db.FutureQueries.Count);

            // access q2 first to trigger loading, testing loading from FutureCount
            // this triggers the loading of all the future queries
            var count = q2.Value;
            Assert.AreNotEqual(count, 0);

            // should be cleared at this point
            //Assert.AreEqual(0, db.FutureQueries.Count);

            // this should already be loaded
            Assert.IsTrue(((IFutureQuery)q1).IsLoaded);

            var users = q1.ToList();
            Assert.IsNotNull(users);
        }

        [Test]
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
            //Assert.AreEqual(3, db.FutureQueries.Count);

            // this triggers the loading of all the future queries
            User user = q1;
            Assert.IsNotNull(user);

            // should be cleared at this point
            //Assert.AreEqual(0, db.FutureQueries.Count);

            // this should already be loaded
            Assert.IsTrue(((IFutureQuery)q2).IsLoaded);

            var count = q2.Value;
            Assert.AreNotEqual(count, 0);

            var tasks = q3.ToList();
            Assert.IsNotNull(tasks);
        }

        [Test]
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
            //Assert.AreEqual(3, db.FutureQueries.Count);

            // access q2 first to trigger loading, testing loading from FutureCount
            // this triggers the loading of all the future queries
            var count = q2.Value;
            Assert.AreNotEqual(count, 0);

            // should be cleared at this point
            //Assert.AreEqual(0, db.FutureQueries.Count);

            // this should already be loaded
            Assert.IsTrue(((IFutureQuery)q1).IsLoaded);

            var users = q1.Value;
            Assert.IsNotNull(users);

            var tasks = q3.ToList();
            Assert.IsNotNull(tasks);

        }

    }
}
