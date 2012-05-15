using System;
using System.Text;
using System.Collections.Generic;
using System.Linq;
using EntityFramework.Extensions;
using FluentAssertions;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Tracker.SqlServer.CodeFirst;
using Tracker.SqlServer.CodeFirst.Entities;

namespace Tracker.SqlServer.Test
{
    [TestClass]
    public class CacheDbContext
    {
        [TestMethod]
        public void SimpleTest()
        {
            var db = new TrackerContext();
            var roles = db.Roles.FromCache();
            roles.Should().NotBeEmpty();

            var roles2 = db.Roles.FromCache();
            roles2.Should().NotBeEmpty();
        }
    }
}
