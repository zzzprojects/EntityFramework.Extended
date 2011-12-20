using System.Diagnostics;
using System.Xml.Serialization;

namespace EntityFramework.Audit
{
    /// <summary>
    /// A class used to hold audit key values.
    /// </summary>
    [XmlRoot(Namespace = AuditLog.AuditNamespace, ElementName = "key")]
    [DebuggerDisplay("Name: {Name}, Value: {Value}")]
    public class AuditKey
    {
        /// <summary>
        /// Gets or sets the name of the property.
        /// </summary>
        /// <value>The name of the property.</value>
        [XmlAttribute("name")]
        public string Name { get; set; }

        /// <summary>
        /// Gets or sets the type of the property.
        /// </summary>
        /// <value>The type of the property.</value>
        [XmlAttribute("type")]
        public string Type { get; set; }

        /// <summary>
        /// Gets or sets the current/changed value of the property.
        /// </summary>
        /// <value>The current value of the property.</value>
        [XmlElement("value")]
        public object Value { get; set; }
    }
}
