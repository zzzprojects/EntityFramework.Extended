using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using EntityFramework.Extensions;
using Tracker.SqlCompact.Entities;

namespace Tracker.SqlCompact.Test
{
    [TestClass]
    public class BatchObjectContext
    {
        //[TestMethod]
        public void Delete()
        {
            var db = new TrackerEntities();
            string emailDomain = "@test.com";
            int count = db.Users.Delete(u => u.Email.Contains(emailDomain));
        }

        //[TestMethod]
        public void Update()
        {
            var db = new TrackerEntities();
            string emailDomain = "@test.com";
            int count = db.Users.Update(
                u => u.Email.Contains(emailDomain),
                u => new User { IsApproved = false, LastActivityDate = DateTime.Now });
        }
    }
}
