using System;
using Xunit;
using EntityFramework.Extensions;
using Tracker.SqlServer.Entities;

namespace Tracker.SqlServer.Test
{
    
    public class BatchObjectContext
    {
        [Fact]
        public void Delete()
        {
            var db = new TrackerEntities();
            string emailDomain = "@test.com";
            int count = db.Users.Delete(u => u.EmailAddress.EndsWith(emailDomain));
        }

        [Fact]
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
