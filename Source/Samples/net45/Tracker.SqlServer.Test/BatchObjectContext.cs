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
        public void DeleteWhereWithExpressionContainingNullParameter()
        {
            // This test verifies that the delete is processed correctly when the where expression uses a parameter with a null parameter
            var db = new TrackerEntities();
            string emailDomain = "@test.com";
            string optionalComparisonString = null;

            int count = db.Users
                .Where(u => (string.IsNullOrEmpty(optionalComparisonString) || u.AvatarType == optionalComparisonString) && u.EmailAddress.EndsWith(emailDomain))
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

        [Fact]
        public void UpdateWithExpressionContainingNullParameter()
        {
            // This test verifies that the update is interpreted correctly when the where expression uses a parameter with a null parameter
            var db = new TrackerEntities();
            string emailDomain = "@test.com";
            string optionalComparisonString = null;

            int count = db.Users
                .Where(u => u.EmailAddress.EndsWith(emailDomain) && (string.IsNullOrEmpty(optionalComparisonString) || u.AvatarType == optionalComparisonString))
                .Update(u => new User { IsApproved = false, LastActivityDate = DateTime.Now });
        }
    }
}
