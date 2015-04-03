using System.Diagnostics;
using System.Runtime.Serialization;
using System.Xml.Serialization;

namespace EntityFramework.Audit
{
    /// <summary>
    /// A class used to hold audit key values.
    /// </summary>
    [XmlRoot(Namespace = AuditLog.AuditNamespace, ElementName = "key")]
    [DataContract(Name = "key", Namespace = AuditLog.AuditNamespace)]
    [DebuggerDisplay("Name: {Name}, Value: {Value}")]
    public class AuditKey
    {
        /// <summary>
        /// Gets or sets the name of the property.
        /// </summary>
        /// <value>The name of the property.</value>
        [XmlElement("name")]
        [DataMember(Name = "name", Order = 0)]
        public string Name { get; set; }

        /// <summary>
        /// Gets or sets the type of the property.
        /// </summary>
        /// <value>The type of the property.</value>
        [XmlElement("type")]
        [DataMember(Name = "type", Order = 1)]
        public string Type { get; set; }

        /// <summary>
        /// Gets or sets the current/changed value of the property.
        /// </summary>
        /// <value>The current value of the property.</value>
        [XmlElement("value")]
        [DataMember(Name = "value", Order = 2)]
        public object Value { get; set; }
    }
}
