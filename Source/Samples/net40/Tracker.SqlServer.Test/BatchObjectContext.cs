using System;
using System.Text;
using System.Collections.Generic;
using System.Linq;
using NUnit.Framework;
using EntityFramework.Extensions;
using Tracker.SqlServer.Entities;
using System.Linq.Dynamic;

namespace Tracker.SqlServer.Test
{
    [TestFixture]
    public class BatchObjectContext
    {
        [Test]
        public void Delete()
        {
            //var db = new TrackerEntities();
            //string emailDomain = "@test.com";
            //int count = db.Users.Delete(u => u.Email.EndsWith(emailDomain));
        }

        [Test]
        public void Update()
        {
            //var db = new TrackerEntities();
            //string emailDomain = "@test.com";
            //int count = db.Users.Update(
            //    u => u.Email.EndsWith(emailDomain),
            //    u => new User { IsApproved = false, LastActivityDate = DateTime.Now });
        }
    }
}
