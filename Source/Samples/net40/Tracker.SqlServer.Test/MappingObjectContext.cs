using System;
using System.Collections.ObjectModel;
using System.Text;
using System.Collections.Generic;
using System.Linq;
using EntityFramework.Mapping;
using EntityFramework.Reflection;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Tracker.SqlServer.CodeFirst;
using Tracker.SqlServer.CodeFirst.Entities;
using Tracker.SqlServer.Entities;
using EntityFramework.Extensions;
using Task = Tracker.SqlServer.Entities.Task;

namespace Tracker.SqlServer.Test
{
    /// <summary>
    /// Summary description for MappingObjectContext
    /// </summary>
    [TestClass]
    public class MappingObjectContext
    {
        [TestMethod]
        public void GetEntityMapTask()
        {
            //var db = new TrackerEntities();
            //var metadata = db.MetadataWorkspace;

            //var map = db.Tasks.GetEntityMap<Task>();

            //Assert.AreEqual("[dbo].[Task]", map.TableName);
        }


        [TestMethod]
        public void GetEntityMapAuditData()
        {
            var db = new TrackerContext();

            var map = db.Audits.ToObjectQuery().GetEntityMap<AuditData>();

            Assert.AreEqual("[dbo].[Audit]", map.TableName);
        }

    }


}
