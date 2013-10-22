using System;
using NUnit.Framework;
using EntityFramework.Extensions;
using Tracker.SqlServer.Entities;

namespace Tracker.SqlServer.Test
{
    [TestFixture]
    public class BatchObjectContext
    {
        [Test]
        public void Delete()
        {
            var db = new TrackerEntities();
            string emailDomain = "@test.com";
            int count = db.Users.Delete(u => u.EmailAddress.EndsWith(emailDomain));
        }

        [Test]
        public void Update()
        {
            var db = new TrackerEntities();
            string emailDomain = "@test.com";
            int count = db.Users.Update(
                u => u.EmailAddress.EndsWith(emailDomain),
                u => new User { IsApproved = false, LastActivityDate = DateTime.Now });
        }
    }
}
