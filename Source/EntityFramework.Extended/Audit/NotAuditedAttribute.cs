using System;
using EntityFramework.Audit;

namespace EntityFramework.Audit
{
    /// <summary>
    /// Indicates that a field in an audited class should not be included in the audit log.
    /// </summary>
    /// <remarks>
    /// Use the NotAuditedAttribute attribute to prevent a field from being included in the audit.
    /// </remarks>
    /// <seealso cref="AuditAttribute"/>
    /// <seealso cref="AlwaysAuditAttribute"/>
    [AttributeUsage(AttributeTargets.Property | AttributeTargets.Field, AllowMultiple = false)]
    public class NotAuditedAttribute : Attribute
    { }
}