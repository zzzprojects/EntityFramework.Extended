using System;
using System.Linq;
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
            int count = db.Users
                .Where(u => u.EmailAddress.EndsWith(emailDomain))
                .Delete();
        }

        [Fact]
        public void Update()
        {
            var db = new TrackerEntities();
            string emailDomain = "@test.com";
            int count = db.Users
                .Where(u => u.EmailAddress.EndsWith(emailDomain))
                .Update(u => new User { IsApproved = false, LastActivityDate = DateTime.Now });
        }
    }
}
