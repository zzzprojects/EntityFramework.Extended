using System;
using System.Reflection;

namespace EntityFramework.Audit
{
    /// <summary>
    /// An attribute to control the output format of the <see cref="AuditProperty"/> values.
    /// </summary>
    /// <example>The following example is used to mask out the password for use in the audit log.
    /// <code><![CDATA[
    /// [Audit]
    /// public partial class User
    /// {
    ///     [AuditPropertyFormat(typeof(CustomFormat), "FormatPassword")]
    ///     public string Password { get; set; }
    /// }
    /// 
    /// public class CustomFormat
    /// {
    ///     // signature can be static object MethodName(AuditPropertyContext).
    ///     public static object FormatPassword(AuditPropertyContext auditProperty)
    ///     {
    ///         string v = auditProperty.Value as string;
    ///         if (string.IsNullOrEmpty(v))
    ///             return value;
    /// 
    ///         return v.Substring(0, 1) + "*****";
    ///     }
    /// }
    /// ]]>
    /// </code>
    /// </example>
    /// <remarks>
    /// The method signature must be <c>static object MethodName(AuditPropertyContext auditProperty)</c>.
    /// </remarks>
    [AttributeUsage(AttributeTargets.Property | AttributeTargets.Field, AllowMultiple = false)]
    public class AuditPropertyFormatAttribute : Attribute
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="AuditPropertyFormatAttribute"/> class.
        /// </summary>
        /// <param name="formatType">The <see cref="System.Type"/> that contains format method.</param>
        /// <param name="methodName">
        /// The name of the method to call to format the value.  Method signature can be either 
        /// <c>static object MethodName(MemberInfo memberInfo, object value)</c> or <c>static object MethodName(object value)</c>.
        /// </param>
        public AuditPropertyFormatAttribute(Type formatType, string methodName)
        {
            FormatType = formatType;
            MethodName = methodName;
        }

        /// <summary>
        /// Gets or sets the <see cref="System.Type"/> that contains format method.
        /// </summary>
        /// <value>The <see cref="System.Type"/> that contains format method.</value>
        public Type FormatType { get; private set; }

        /// <summary>
        /// Gets or sets the name of the method to call to format the value. Must be a static method.
        /// </summary>
        /// <value>The name of the method to call to format the value.</value>
        /// <remarks>
        /// The method signature must be <c>static object MethodName(AuditPropertyContext auditProperty)</c>.
        /// </remarks>
        public string MethodName { get; private set; }
    }

    /// <summary>
    /// The audit property context.
    /// </summary>
    public class AuditPropertyContext
    {
        /// <summary>
        /// Gets or sets the entity.
        /// </summary>
        public object Entity { get; set; }
        /// <summary>
        /// Gets or sets the value.
        /// </summary>
        public object Value { get; set; }
        /// <summary>
        /// Gets or sets the type of the value.
        /// </summary>
        public Type ValueType { get; set; }
    }
}
