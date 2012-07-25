using System;
using System.Data.Entity.Infrastructure;
using System.Data.Objects;
using System.Linq;
using EntityFramework.Audit;
using EntityFramework.Extensions;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Tracker.SqlServer.CodeFirst;
using Tracker.SqlServer.CodeFirst.Entities;

namespace Tracker.SqlServer.Test
{
    [TestClass]
    public class AuditTest
    {
        [TestMethod]
        public void CreateLog()
        {
            var auditConfiguration = AuditConfiguration.Default;

            auditConfiguration.IncludeRelationships = true;
            auditConfiguration.LoadRelationships = true;
            auditConfiguration.DefaultAuditable = true;

            // customize the audit for Task entity
            auditConfiguration.IsAuditable<Task>()
              .NotAudited(t => t.TaskExtended)
              .FormatWith(t => t.Status, v => FormatStatus(v));

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
            Assert.IsNotNull(log);

            string xml = log.ToXml();
            Assert.IsNotNull(xml);
        }

        [TestMethod]
        public void CreateLog2()
        {
            AuditConfiguration.Default.IncludeRelationships = true;
            AuditConfiguration.Default.LoadRelationships = true;

            AuditConfiguration.Default.IsAuditable<Task>();
            AuditConfiguration.Default.IsAuditable<User>();

            var db = new TrackerContext();
            var audit = db.BeginAudit();

            var task = db.Tasks.Find(1);
            Assert.IsNotNull(task);

            task.PriorityId = 2;
            task.StatusId = 2;
            task.Summary = "Summary: " + DateTime.Now.Ticks;

            var log = audit.CreateLog();
            Assert.IsNotNull(log);

            string xml = log.ToXml();
            Assert.IsNotNull(xml);
        }

        [TestMethod]
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
            Assert.IsNotNull(log);

            string beforeXml = log.ToXml();
            Assert.IsNotNull(beforeXml);

            db.SaveChanges();

            log.Refresh();

            string afterXml = log.ToXml();
            Assert.IsNotNull(afterXml);
        }

        [TestMethod]
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
            Assert.IsNotNull(log);

            string beforeXml = log.ToXml();
            Assert.IsNotNull(beforeXml);

            db.SaveChanges();

            log.Refresh();

            string afterXml = log.ToXml();
            Assert.IsNotNull(afterXml);

            var lastLog = audit.LastLog;
            var lastXml = lastLog.Refresh().ToXml();

            Assert.IsNotNull(lastXml);

        }

        [TestMethod]
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
            Assert.IsNotNull(log);

            string beforeXml = log.ToXml();
            Assert.IsNotNull(beforeXml);
            Console.WriteLine(beforeXml);

            db.SaveChanges();

            log.Refresh();

            string afterXml = log.ToXml();
            Assert.IsNotNull(afterXml);

            var lastLog = audit.LastLog;
            var lastXml = lastLog.Refresh().ToXml();

            Assert.IsNotNull(lastXml);
            Console.WriteLine(lastXml);

            db.Users.Remove(user);

            var deleteLog = audit.CreateLog();
            Assert.IsNotNull(deleteLog);

            db.SaveChanges();

            var deleteXml = deleteLog.ToXml();
            Assert.IsNotNull(deleteXml);
            Console.WriteLine(deleteXml);
        }

        [TestMethod]
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
            Assert.IsNotNull(log);

            string beforeXml = log.ToXml();
            Assert.IsNotNull(beforeXml);

            db.SaveChanges();

            log.Refresh();

            string afterXml = log.ToXml();
            Assert.IsNotNull(afterXml);

            var lastLog = audit.LastLog;
            var lastXml = lastLog.Refresh().ToXml();

            Assert.IsNotNull(lastXml);
            Console.WriteLine(lastXml);

            user.EmailAddress = string.Format("update.{0}@test.com", DateTime.Now.Ticks);

            var updateLog = audit.CreateLog();
            Assert.IsNotNull(updateLog);

            db.SaveChanges();

            var deleteXml = updateLog.ToXml();
            Assert.IsNotNull(deleteXml);
            Console.WriteLine(deleteXml);
        }


        public static object FormatStatus(AuditPropertyContext auditProperty)
        {
            Console.WriteLine("FormatStatus: {0}", auditProperty.Value);
            return "Status: " + auditProperty.Value;
        }
    }
}
