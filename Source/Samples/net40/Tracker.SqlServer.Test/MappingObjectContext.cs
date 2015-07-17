using System;
using EntityFramework.Mapping;
using Xunit;
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
    
    public class MappingObjectContext
    {
        [Fact]
        public void GetEntityMapTask()
        {
            //var db = new TrackerEntities();
            //var metadata = db.MetadataWorkspace;

            //var map = db.Tasks.GetEntityMap<Task>();

            //Assert.Equal("[dbo].[Task]", map.TableName);
        }


        [Fact]
        public void GetEntityMapAuditData()
        {
            var db = new TrackerContext();
            var resolver = new MetadataMappingProvider();

            var map = resolver.GetEntityMap(typeof(AuditData), db);

            //var map = db.Audits.ToObjectQuery().GetEntityMap<AuditData>();

            Assert.Equal("dbo",map.SchemaName);
            Assert.Equal("Audit", map.TableName);
        }


        [Fact]
        public void GetInheritedEntityMapAuditData()
        {
            var db = new TrackerContext();
            var resolver = new MetadataMappingProvider();

            var map = resolver.GetEntityMap(typeof(CodeFirst.Entities.Task), db);

            //var map = db.Audits.ToObjectQuery().GetEntityMap<AuditData>();

            Assert.Equal("dbo", map.SchemaName);
            Assert.Equal("Task", map.TableName);
        }

    }


}
