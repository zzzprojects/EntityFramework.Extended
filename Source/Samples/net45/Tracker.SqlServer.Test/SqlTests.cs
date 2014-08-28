using System;
using System.Collections.Generic;
using System.Data.Entity.Infrastructure;
using System.Linq;
using FluentAssertions;
using Xunit;
using Tracker.SqlServer.CodeFirst;
using Tracker.SqlServer.CodeFirst.Entities;

namespace EntityFramework.Test
{    
    public class SqlTests
    {
        [Fact]
        public void SelectByKey()
        {
            var db = new TrackerContext();
            var contextAdapter = db as IObjectContextAdapter;
            var objectContext = contextAdapter.ObjectContext;

            var sql = "SELECT VALUE U.EmailAddress FROM TrackerContext.Users AS U WHERE U.Id = 1";

            var q = objectContext.CreateQuery<object>(sql);
            var list = q.FirstOrDefault();

            list.Should().NotBeNull();


        }
    }
}
