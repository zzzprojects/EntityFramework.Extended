using System;
using System.Data.Entity.Core.Objects.DataClasses;
using System.Data.Entity.Infrastructure;
using System.IO;
using System.Linq;
using System.Runtime.Serialization;
using System.Text;
using System.Xml;
using EntityFramework.Audit;
using EntityFramework.Extensions;
using FluentAssertions;
using Newtonsoft.Json;
using Newtonsoft.Json.Converters;
using Xunit;
using Tracker.SqlServer.CodeFirst;
using Tracker.SqlServer.CodeFirst.Entities;
using System.Data.Entity;

namespace Tracker.SqlServer.Test
{

    public class AuditTest
    {
        [Fact]
        public void CreateLogFormatWith()
        {
            var auditConfiguration = AuditConfiguration.Default;

            auditConfiguration.IncludeRelationships = true;
            auditConfiguration.LoadRelationships = true;
            auditConfiguration.DefaultAuditable = true;

            // customize the audit for Task entity
            auditConfiguration.IsAuditable<Task>()
              .NotAudited(t => t.TaskExtended)
              .FormatWith(t => t.Status, v => FormatStatus(v));

            var db = new TrackerContext();
            var audit = db.BeginAudit();

            var user = db.Users.Find(1);
            user.Comment = "Testing: " + DateTime.Now.Ticks;

            var task = new Task()
            {
                AssignedId = 1,
                CreatedId = 1,
                StatusId = 1,
                PriorityId = 2,
                Summary = "Summary: " + DateTime.Now.Ticks
            };
            db.Tasks.Add(task);

            var task2 = db.Tasks.Find(1);
            task2.PriorityId = 2;
            task2.StatusId = 2;
            task2.Summary = "Summary: " + DateTime.Now.Ticks;

            var log = audit.CreateLog();
            Assert.NotNull(log);

            string xml = log.ToXml();
            Assert.NotNull(xml);

            foreach (var property in log.Entities.SelectMany(e => e.Properties))
            {
                Assert.NotEqual(property.Current, "{error}");
                Assert.NotEqual(property.Original, "{error}");
            }
        }

        [Fact]
        public void CreateLogLoaded()
        {
            var auditConfiguration = AuditConfiguration.Default;

            auditConfiguration.IncludeRelationships = true;
            auditConfiguration.LoadRelationships = true;
            auditConfiguration.DefaultAuditable = true;

            // customize the audit for Task entity
            //auditConfiguration.IsAuditable<Task>()
            //  .NotAudited(t => t.TaskExtended)
            //  .FormatWith(t => t.Status, v => FormatStatus(v));

            // set name as the display member when status is a foreign key
            auditConfiguration.IsAuditable<Status>()
              .DisplayMember(t => t.Name);

            var db = new TrackerContext();
            var audit = db.BeginAudit();

            var user = db.Users.Find(1);
            user.Comment = "Testing: " + DateTime.Now.Ticks;

            var newTask = new Task()
            {
                AssignedId = 1,
                CreatedId = 1,
                StatusId = 1,
                PriorityId = 2,
                Summary = "Summary: " + DateTime.Now.Ticks
            };
            db.Tasks.Add(newTask);

            var p = db.Priorities.Find(2);

            var updateTask = db.Tasks.Find(1);
            updateTask.Priority = p;
            updateTask.StatusId = 2;
            updateTask.Summary = "Summary: " + DateTime.Now.Ticks;

            var log = audit.CreateLog();
            Assert.NotNull(log);

            string xml = log.ToXml();
            Assert.NotNull(xml);

            foreach (var property in log.Entities.SelectMany(e => e.Properties))
            {
                Assert.NotEqual(property.Current, "{error}");
                Assert.NotEqual(property.Original, "{error}");
            }
        }

        [Fact]
        public void CreateLog()
        {
            var auditConfiguration = AuditConfiguration.Default;

            auditConfiguration.IncludeRelationships = true;
            auditConfiguration.LoadRelationships = true;
            auditConfiguration.DefaultAuditable = true;

            // customize the audit for Task entity
            //auditConfiguration.IsAuditable<Task>()
            //  .NotAudited(t => t.TaskExtended)
            //  .FormatWith(t => t.Status, v => FormatStatus(v));

            // set name as the display member when status is a foreign key
            auditConfiguration.IsAuditable<Status>()
              .DisplayMember(t => t.Name);

            var db = new TrackerContext();
            var audit = db.BeginAudit();

            var user = db.Users.Find(1);
            user.Comment = "Testing: " + DateTime.Now.Ticks;

            var task = new Task()
            {
                AssignedId = 1,
                CreatedId = 1,
                StatusId = 1,
                PriorityId = 2,
                Summary = "Summary: " + DateTime.Now.Ticks
            };
            db.Tasks.Add(task);

            var task2 = db.Tasks.Find(1);
            task2.PriorityId = 2;
            task2.StatusId = 2;
            task2.Summary = "Summary: " + DateTime.Now.Ticks;

            var log = audit.CreateLog();
            Assert.NotNull(log);

            string xml = log.ToXml();
            Assert.NotNull(xml);

            foreach (var property in log.Entities.SelectMany(e => e.Properties))
            {
                Assert.NotEqual(property.Current, "{error}");
                Assert.NotEqual(property.Original, "{error}");
            }
        }

        [Fact]
        public void CreateLog2()
        {
            AuditConfiguration.Default.IncludeRelationships = true;
            AuditConfiguration.Default.LoadRelationships = true;

            AuditConfiguration.Default.IsAuditable<Task>();
            AuditConfiguration.Default.IsAuditable<User>();

            var db = new TrackerContext();
            var audit = db.BeginAudit();

            var task = db.Tasks.Find(1);
            Assert.NotNull(task);

            task.PriorityId = 2;
            task.StatusId = 2;
            task.Summary = "Summary: " + DateTime.Now.Ticks;

            var log = audit.CreateLog();
            Assert.NotNull(log);
            foreach (var property in log.Entities.SelectMany(e => e.Properties))
            {
                Assert.NotEqual(property.Current, "{error}");
                Assert.NotEqual(property.Original, "{error}");
            }

            string xml = log.ToXml();
            Assert.NotNull(xml);
        }

        [Fact]
        public void CreateLog3()
        {
            AuditConfiguration.Default.IncludeRelationships = true;
            AuditConfiguration.Default.LoadRelationships = true;

            AuditConfiguration.Default.IsAuditable<Task>();
            AuditConfiguration.Default.IsAuditable<User>();

            var db = new TrackerContext();
            var audit = db.BeginAudit();

            var user = new User();
            user.EmailAddress = string.Format("email.{0}@test.com", DateTime.Now.Ticks);
            user.CreatedDate = DateTime.Now;
            user.ModifiedDate = DateTime.Now;
            user.PasswordHash = DateTime.Now.Ticks.ToString();
            user.PasswordSalt = "abcde";
            user.IsApproved = false;
            user.LastActivityDate = DateTime.Now;

            db.Users.Add(user);

            var log = audit.CreateLog();
            Assert.NotNull(log);
            foreach (var property in log.Entities.SelectMany(e => e.Properties))
            {
                Assert.NotEqual(property.Current, "{error}");
                Assert.NotEqual(property.Original, "{error}");
            }

            string beforeXml = log.ToXml();
            Assert.NotNull(beforeXml);

            db.SaveChanges();

            log.Refresh();

            string afterXml = log.ToXml();
            Assert.NotNull(afterXml);
        }

        [Fact]
        public void Refresh()
        {
            AuditConfiguration.Default.IncludeRelationships = true;
            AuditConfiguration.Default.LoadRelationships = true;

            AuditConfiguration.Default.IsAuditable<Task>();
            AuditConfiguration.Default.IsAuditable<User>().NotAudited(p => p.Avatar);

            var db = new TrackerContext();
            var audit = db.BeginAudit();

            var user = new User();
            user.EmailAddress = string.Format("email.{0}@test.com", DateTime.Now.Ticks);
            user.CreatedDate = DateTime.Now;
            user.ModifiedDate = DateTime.Now;
            user.PasswordHash = DateTime.Now.Ticks.ToString();
            user.PasswordSalt = "abcde";
            user.IsApproved = false;
            user.LastActivityDate = DateTime.Now;

            db.Users.Add(user);

            var log = audit.CreateLog();
            Assert.NotNull(log);
            foreach (var property in log.Entities.SelectMany(e => e.Properties))
            {
                Assert.NotEqual(property.Current, "{error}");
                Assert.NotEqual(property.Original, "{error}");
            }

            string beforeXml = log.ToXml();
            Assert.NotNull(beforeXml);

            db.SaveChanges();

            log.Refresh();
            foreach (var property in log.Entities.SelectMany(e => e.Properties))
            {
                Assert.NotEqual(property.Current, "{error}");
                Assert.NotEqual(property.Original, "{error}");
            }

            string afterXml = log.ToXml();
            Assert.NotNull(afterXml);

            var lastLog = audit.LastLog;
            var lastXml = lastLog.Refresh().ToXml();

            Assert.NotNull(lastXml);

        }

        [Fact]
        public void Delete()
        {
            AuditConfiguration.Default.IncludeRelationships = true;
            AuditConfiguration.Default.LoadRelationships = true;

            AuditConfiguration.Default.IsAuditable<Task>();
            AuditConfiguration.Default.IsAuditable<User>().NotAudited(p => p.Avatar);

            var db = new TrackerContext();
            var audit = db.BeginAudit();

            var user = new User();
            user.EmailAddress = string.Format("email.{0}@test.com", DateTime.Now.Ticks);
            user.CreatedDate = DateTime.Now;
            user.ModifiedDate = DateTime.Now;
            user.PasswordHash = DateTime.Now.Ticks.ToString();
            user.PasswordSalt = "abcde";
            user.IsApproved = false;
            user.LastActivityDate = DateTime.Now;

            db.Users.Add(user);

            var log = audit.CreateLog();
            Assert.NotNull(log);

            string beforeXml = log.ToXml();
            Assert.NotNull(beforeXml);
            Console.WriteLine(beforeXml);

            db.SaveChanges();

            log.Refresh();
            foreach (var property in log.Entities.SelectMany(e => e.Properties))
            {
                Assert.NotEqual(property.Current, "{error}");
                Assert.NotEqual(property.Original, "{error}");
            }

            string afterXml = log.ToXml();
            Assert.NotNull(afterXml);

            var lastLog = audit.LastLog;
            var lastXml = lastLog.Refresh().ToXml();

            Assert.NotNull(lastXml);
            Console.WriteLine(lastXml);

            db.Users.Remove(user);

            var deleteLog = audit.CreateLog();
            Assert.NotNull(deleteLog);

            db.SaveChanges();

            var deleteXml = deleteLog.ToXml();
            Assert.NotNull(deleteXml);
            Console.WriteLine(deleteXml);
        }

        [Fact]
        public void Update()
        {
            AuditConfiguration.Default.IncludeRelationships = true;
            AuditConfiguration.Default.LoadRelationships = true;

            AuditConfiguration.Default.IsAuditable<Task>();
            AuditConfiguration.Default.IsAuditable<User>().NotAudited(p => p.Avatar);

            var db = new TrackerContext();
            var audit = db.BeginAudit();

            var user = new User();
            user.EmailAddress = string.Format("email.{0}@test.com", DateTime.Now.Ticks);
            user.CreatedDate = DateTime.Now;
            user.ModifiedDate = DateTime.Now;
            user.PasswordHash = DateTime.Now.Ticks.ToString();
            user.PasswordSalt = "abcde";
            user.IsApproved = false;
            user.LastActivityDate = DateTime.Now;

            db.Users.Add(user);

            var log = audit.CreateLog();
            Assert.NotNull(log);

            string beforeXml = log.ToXml();
            Assert.NotNull(beforeXml);

            db.SaveChanges();

            log.Refresh();
            foreach (var property in log.Entities.SelectMany(e => e.Properties))
            {
                Assert.NotEqual(property.Current, "{error}");
                Assert.NotEqual(property.Original, "{error}");
            }

            string afterXml = log.ToXml();
            Assert.NotNull(afterXml);

            var lastLog = audit.LastLog;
            var lastXml = lastLog.Refresh().ToXml();

            Assert.NotNull(lastXml);
            Console.WriteLine(lastXml);

            user.EmailAddress = string.Format("update.{0}@test.com", DateTime.Now.Ticks);

            var updateLog = audit.CreateLog();
            Assert.NotNull(updateLog);
            foreach (var property in log.Entities.SelectMany(e => e.Properties))
            {
                Assert.NotEqual(property.Current, "{error}");
                Assert.NotEqual(property.Original, "{error}");
            }

            db.SaveChanges();

            var updateXml = updateLog.ToXml();
            Assert.NotNull(updateXml);
            Console.WriteLine(updateXml);
        }

        [Fact]
        public void MaintainAcrossSaves()
        {
            var auditConfiguration = AuditConfiguration.Default;

            auditConfiguration.IncludeRelationships = true;
            auditConfiguration.LoadRelationships = true;
            auditConfiguration.DefaultAuditable = true;
            auditConfiguration.MaintainAcrossSaves = true;

            // customize the audit for Task entity
            //auditConfiguration.IsAuditable<Task>()
            //  .NotAudited(t => t.TaskExtended)
            //  .FormatWith(t => t.Status, v => FormatStatus(v));

            // set name as the display member when status is a foreign key
            auditConfiguration.IsAuditable<Status>()
              .DisplayMember(t => t.Name);

            var db = new TrackerContext();
            var tran = db.Database.BeginTransaction();
            var audit = db.BeginAudit();

            var user = db.Users.Find(1);
            user.Comment = "Testing: " + DateTime.Now.Ticks;

            var task = new Task()
            {
                AssignedId = 1,
                StatusId = 1,
                PriorityId = 2,
                Summary = "Summary: " + DateTime.Now.Ticks,
                CreatedId = 1,
                CreatedDate = DateTime.Now,
                ModifiedDate = DateTime.Now
            };
            db.Tasks.Add(task);

            db.SaveChanges();

            Assert.NotNull(audit.LastLog);
            Assert.Equal(2, audit.LastLog.Entities.Count);


            var task2 = db.Tasks.Find(1);
            task2.PriorityId = 2;
            task2.StatusId = 2;
            task2.Summary = "Summary: " + DateTime.Now.Ticks;

            db.SaveChanges();

            Assert.NotNull(audit.LastLog);
            Assert.Equal(3, audit.LastLog.Entities.Count);

            var log = audit.LastLog;
            Assert.NotNull(log);

            string xml = log.ToXml();
            Assert.NotNull(xml);

            foreach (var property in log.Entities.SelectMany(e => e.Properties))
            {
                Assert.NotEqual(property.Current, "{error}");
                Assert.NotEqual(property.Original, "{error}");
            }

            //undo work
            tran.Rollback();
        }

        [Fact]
        public void CompareXml()
        {
            var auditConfiguration = AuditConfiguration.Default;

            auditConfiguration.IncludeRelationships = true;
            auditConfiguration.LoadRelationships = true;
            auditConfiguration.DefaultAuditable = true;

            // customize the audit for Task entity
            //auditConfiguration.IsAuditable<Task>()
            //  .NotAudited(t => t.TaskExtended)
            //  .FormatWith(t => t.Status, v => FormatStatus(v));

            // set name as the display member when status is a foreign key
            auditConfiguration.IsAuditable<Status>()
              .DisplayMember(t => t.Name);

            var db = new TrackerContext();
            var audit = db.BeginAudit();

            var user = db.Users.Find(1);
            user.Comment = "Testing: " + DateTime.Now.Ticks;

            var task = new Task()
            {
                AssignedId = 1,
                CreatedId = 1,
                StatusId = 1,
                PriorityId = 2,
                Summary = "Summary: " + DateTime.Now.Ticks
            };
            db.Tasks.Add(task);

            var task2 = db.Tasks.Find(1);
            task2.PriorityId = 2;
            task2.StatusId = 2;
            task2.Summary = "Summary: " + DateTime.Now.Ticks;

            var log = audit.CreateLog();
            Assert.NotNull(log);

            string xml = log.ToXml();
            Assert.NotNull(xml);
            File.WriteAllText(@"test.xml.xml", xml);

            foreach (var property in log.Entities.SelectMany(e => e.Properties))
            {
                Assert.NotEqual(property.Current, "{error}");
                Assert.NotEqual(property.Original, "{error}");
            }

            var builder = new StringBuilder();
            var settings = new XmlWriterSettings { Indent = true, OmitXmlDeclaration = true };
            var writer = XmlWriter.Create(builder, settings);

            var serializer = new DataContractSerializer(typeof(AuditLog));
            serializer.WriteStartObject(writer, log);
            writer.WriteAttributeString("xmlns", "xsd", null, "http://www.w3.org/2001/XMLSchema");
            serializer.WriteObjectContent(writer, log);
            serializer.WriteEndObject(writer);

            writer.Flush();

            string xml2 = builder.ToString();
            File.WriteAllText(@"test.data.xml", xml2);

            string json = JsonConvert.SerializeObject(log, Newtonsoft.Json.Formatting.Indented, new StringEnumConverter());
            File.WriteAllText(@"test.data.json", json);


        }

        [Fact]
        public void LogWithNullableRelations()
        {
            var auditConfiguration = AuditConfiguration.Default;

            auditConfiguration.IncludeRelationships = true;
            auditConfiguration.LoadRelationships = true;
            auditConfiguration.DefaultAuditable = true;
            auditConfiguration.MaintainAcrossSaves = false;

            auditConfiguration.IsAuditable<Task>();
            auditConfiguration.IsAuditable<Priority>().DisplayMember(t => t.Name);

            var db = new TrackerContext();
            var tran = db.Database.BeginTransaction();
            var audit = db.BeginAudit();

            var task = new Task()
            {
                AssignedId = 1,
                StatusId = 1,
                PriorityId = 2,
                Summary = "Summary: " + DateTime.Now.Ticks,
                CreatedId = 1,
                CreatedDate = DateTime.Now,
                ModifiedDate = DateTime.Now,

            };
            db.Tasks.Add(task);
            db.SaveChanges();

            foreach (var property in audit.LastLog.Entities.SelectMany(e => e.Properties))
            {
                Assert.NotEqual(property.Current, "{error}");
                Assert.NotEqual(property.Original, "{error}");
            }

            task.PriorityId = null;
            db.SaveChanges();

            foreach (var property in audit.LastLog.Entities.SelectMany(e => e.Properties))
            {
                Assert.NotEqual(property.Current, "{error}");
                Assert.NotEqual(property.Original, "{error}");
            }

            task.PriorityId = 1;
            db.SaveChanges();

            foreach (var property in audit.LastLog.Entities.SelectMany(e => e.Properties))
            {
                Assert.NotEqual(property.Current, "{error}");
                Assert.NotEqual(property.Original, "{error}");
            }

            task.PriorityId = 2;
            db.SaveChanges();

            foreach (var property in audit.LastLog.Entities.SelectMany(e => e.Properties))
            {
                Assert.NotEqual(property.Current, "{error}");
                Assert.NotEqual(property.Original, "{error}");
            }

            //undo work
            tran.Rollback();
        }

        [Fact]
        public void LogWithNullableRelationWithoutValueAndAllreadyLoadedRelation()
        {
            var auditConfiguration = AuditConfiguration.Default;

            auditConfiguration.IncludeRelationships = true;
            auditConfiguration.LoadRelationships = true;
            auditConfiguration.DefaultAuditable = true;
            auditConfiguration.MaintainAcrossSaves = false;

            auditConfiguration.IsAuditable<Task>();
            auditConfiguration.IsAuditable<Priority>().DisplayMember(t => t.Name);

            var db = new TrackerContext();
            var tran = db.Database.BeginTransaction();
            var audit = db.BeginAudit();

            var task = new Task()
            {
                AssignedId = 1,
                StatusId = 1,
                Priority = null,
                Summary = "Summary: " + DateTime.Now.Ticks,
                CreatedId = 1,
                CreatedDate = DateTime.Now,
                ModifiedDate = DateTime.Now,
            };
            db.Tasks.Add(task);

            var entries = ((IObjectContextAdapter)db).ObjectContext.ObjectStateManager.GetObjectStateEntries(EntityState.Added);
            var relation = entries.First().RelationshipManager.GetRelatedReference<Priority>("Tracker.SqlServer.CodeFirst.Task_Priority", "Task_Priority_Target");
            relation.Load();

            db.SaveChanges();

            foreach (var property in audit.LastLog.Entities.SelectMany(e => e.Properties))
            {
                Assert.NotEqual(property.Current, "{error}");
                Assert.NotEqual(property.Original, "{error}");
            }

            //undo work
            tran.Rollback();
        }

        public static object FormatStatus(AuditPropertyContext auditProperty)
        {
            Console.WriteLine("FormatStatus: {0}", auditProperty.Value);
            return "Status: " + auditProperty.Value;
        }


    }
}
