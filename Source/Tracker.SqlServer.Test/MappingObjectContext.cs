using System;
using System.Collections.ObjectModel;
using System.Data.Metadata.Edm;
using System.Text;
using System.Collections.Generic;
using System.Linq;
using EntityFramework.Mapping;
using EntityFramework.Reflection;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Tracker.SqlServer.Entities;
using EntityFramework.Extensions;

namespace Tracker.SqlServer.Test
{
    /// <summary>
    /// Summary description for MappingObjectContext
    /// </summary>
    [TestClass]
    public class MappingObjectContext
    {
        [TestMethod]
        public void TestMethod1()
        {
            var db = new TrackerEntities();
            var metadata = db.MetadataWorkspace;

            var map = db.Tasks.GetEntityMap<Task>();

        }

    }


}
