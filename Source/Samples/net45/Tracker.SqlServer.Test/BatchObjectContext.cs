using System;
using System.Data.Objects;
using System.Text;
using System.Collections.Generic;
using System.Linq;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using EntityFramework.Extensions;
using Tracker.SqlServer.Entities;
using System.Linq.Dynamic;

namespace Tracker.SqlServer.Test
{
    [TestClass]
    public class BatchObjectContext
    {
        [TestMethod]
        public void Delete()
        {
            var db = new TrackerEntities();
            string emailDomain = "@test.com";
            int count = db.Users.Delete(u => u.Email.EndsWith(emailDomain));
        }

        [TestMethod]
        public void DeleteWithExpressionContainingNullParameter()
        {
            // This test verifies that the delete is processed correctly when the where expression uses a parameter with a null parameter
            var db = new TrackerEntities();
            string emailDomain = "@test.com";
            string nullComparisonString = null;

            int count = db.Users.Delete(u => u.Email.EndsWith(emailDomain) && u.AvatarType == nullComparisonString);
        }

        [TestMethod]
        public void DeleteWhereWithExpressionContainingNullParameter()
        {
            // This test verifies that the delete is processed correctly when the where expression uses a parameter with a null parameter
            var db = new TrackerEntities();
            string emailDomain = "@test.com";
            string nullComparisonString = null; 
            
            int count = db.Users
                .Where(u => u.AvatarType == nullComparisonString && u.Email.EndsWith(emailDomain))
                .Delete();
        }

        [TestMethod]
        public void Update()
        {
            var db = new TrackerEntities();
            string emailDomain = "@test.com";
            int count = db.Users.Update(
                u => u.Email.EndsWith(emailDomain),
                u => new User { IsApproved = false, LastActivityDate = DateTime.Now });
        }

        [TestMethod]
        public void UpdateWithExpressionContainingNullParameter()
        {
            // This test verifies that the update is interpreted correctly when the where expression uses a parameter with a null parameter
            var db = new TrackerEntities();
            string emailDomain = "@test.com";
            string nullComparisonString = null;

            int count = db.Users.Update(
                u => u.Email.EndsWith(emailDomain) && u.AvatarType == nullComparisonString,
                u => new User { IsApproved = false, LastActivityDate = DateTime.Now });
        }
    }
}
