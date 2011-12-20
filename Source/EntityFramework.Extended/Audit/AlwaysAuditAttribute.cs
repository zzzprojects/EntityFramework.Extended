using System;

namespace EntityFramework.Audit
{
    /// <summary>
    /// Indicates that a field in an audited class should always be included in the audit data even if it hasn't been changed.
    /// </summary>
    /// <seealso cref="AuditAttribute"/>
    /// <seealso cref="NotAuditedAttribute"/>
    [AttributeUsage(AttributeTargets.Property | AttributeTargets.Field, AllowMultiple = false)]
    public class AlwaysAuditAttribute : Attribute
    { }
}