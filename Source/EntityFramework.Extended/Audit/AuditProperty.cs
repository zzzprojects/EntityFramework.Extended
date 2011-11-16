using System.ComponentModel;
using System.Diagnostics;
using System.Runtime.Serialization;
using System.Xml.Serialization;

namespace EntityFramework.Audit
{
    /// <summary>
    /// A class for logging the changes to a property on an entity.
    /// </summary>
    [XmlRoot(Namespace = AuditLog.AuditNamespace, ElementName = "property")]
    [DebuggerDisplay("Name: {Name}, Type: {Type}")]
    public class AuditProperty
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
        /// Gets or sets a value indicating whether this property is an association.
        /// </summary>
        /// <value>
        /// 	<c>true</c> if this property is an association; otherwise, <c>false</c>.
        /// </value>
        [XmlAttribute("isRelationship")]
        [DefaultValue(false)]
        public bool IsRelationship { get; set; }

        /// <summary>
        /// Gets or sets the property names that this association maps to.
        /// </summary>
        /// <value>The property names that this association maps to..</value>
        [XmlAttribute("foreignKey")]
        public string ForeignKey { get; set; }

        /// <summary>
        /// Gets or sets the current/changed value of the property.
        /// </summary>
        /// <value>The current value of the property.</value>
        [XmlElement("current")]
        public object Current { get; set; }

        /// <summary>
        /// Gets or sets the original value of the property.
        /// </summary>
        /// <value>The original value of the property.</value>
        [XmlElement("original")]
        public object Original { get; set; }
    }
}