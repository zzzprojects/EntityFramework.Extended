using System;
using EntityFramework.Mapping;
using NUnit.Framework;
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
    [TestFixture]
    public class MappingObjectContext
    {
        [Test]
        public void GetEntityMapTask()
        {
            //var db = new TrackerEntities();
            //var metadata = db.MetadataWorkspace;

            //var map = db.Tasks.GetEntityMap<Task>();

            //Assert.AreEqual("[dbo].[Task]", map.TableName);
        }


        [Test]
        public void GetEntityMapAuditData()
        {
            var db = new TrackerContext();
            var resolver = new MetadataMappingProvider();

            var map = resolver.GetEntityMap(typeof(AuditData), db);

            //var map = db.Audits.ToObjectQuery().GetEntityMap<AuditData>();

            Assert.AreEqual("[dbo].[Audit]", map.TableName);
        }


        [Test]
        public void GetInheritedEntityMapAuditData()
        {
            var db = new TrackerContext();
            var resolver = new MetadataMappingProvider();

            var map = resolver.GetEntityMap(typeof(CodeFirst.Entities.Task), db);

            //var map = db.Audits.ToObjectQuery().GetEntityMap<AuditData>();

            Assert.AreEqual("[dbo].[Task]", map.TableName);
        }

    }


}
