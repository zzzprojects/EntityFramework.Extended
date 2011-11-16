using System;
using EntityFramework.Audit;

namespace EntityFramework.Audit
{
    /// <summary>
    /// Indicates that a class can be audited.
    /// </summary>
    /// <remarks>
    /// Use the <see cref="NotAuditedAttribute"/> attribute to prevent a field from being included in the audit
    /// </remarks>
    /// <seealso cref="NotAuditedAttribute"/>
    /// <seealso cref="AlwaysAuditAttribute"/>
    [AttributeUsage(AttributeTargets.Struct | AttributeTargets.Class, AllowMultiple = false)]
    public class AuditAttribute : Attribute
    { }
}