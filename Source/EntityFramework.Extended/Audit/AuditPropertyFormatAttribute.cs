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
    ///     // signature can be either static object MethodName(MemberInfo memberInfo, object value) or static object MethodName(object value).
    ///     public static object FormatPassword(MemberInfo memberInfo, object value)
    ///     {
    ///         string v = value as string;
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
    /// The method signature can be either <c>static object MethodName(MemberInfo memberInfo, object value)</c>
    ///  or <c>static object MethodName(object value)</c>.
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
        /// The method signature can be either <c>static object MethodName(MemberInfo memberInfo, object value)</c>
        ///  or <c>static object MethodName(object value)</c>.
        /// </remarks>
        public string MethodName { get; private set; }
    }

    public class AuditPropertyContext
    {
        public object Entity { get; set; }
        public object Value { get; set; }
        public Type ValueType { get; set; }
    }
}
