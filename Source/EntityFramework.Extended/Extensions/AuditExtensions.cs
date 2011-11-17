using System;
using System.Data.Entity;
using System.Data.Objects;
using EntityFramework.Audit;

namespace EntityFramework.Extensions
{
    public static class AuditExtensions
    {
        public static AuditLogger BeginAudit(this ObjectContext objectContext, AuditConfiguration configuration = null)
        {
            return new AuditLogger(objectContext, configuration);
        }

        public static AuditLogger BeginAudit(this DbContext dbContext, AuditConfiguration configuration = null)
        {
            return new AuditLogger(dbContext, configuration);
        }
    }
}