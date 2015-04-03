using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Data.Entity.Infrastructure;
using Tracker.SqlServer.CodeFirst.Entities;
using Tracker.SqlServer.CodeFirst.Mapping;

namespace Tracker.SqlServer.CodeFirst
{
    public partial class TrackerContext
    {
        public override int SaveChanges()
        {
            return base.SaveChanges();
        }
    }
}