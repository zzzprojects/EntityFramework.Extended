using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Xml;
using System.Xml.Serialization;
using EntityFramework.Reflection;

namespace EntityFramework.Audit
{
    /// <summary>
    /// A class representing a log of the changes.
    /// </summary>
    [XmlRoot(Namespace = AuditNamespace, ElementName = "audit")]
    public class AuditLog
    {
        /// <summary>
        /// The schema namespace for the audit log.
        /// </summary>
        public const string AuditNamespace = "http://schemas.tempuri.org/ef/audit/1.0";

        private static readonly Lazy<XmlSerializer> _serializer;
        private readonly List<AuditEntity> _entities;

        static AuditLog()
        {
            _serializer = new Lazy<XmlSerializer>(() =>
              new XmlSerializer(typeof(AuditLog), AuditNamespace));
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="AuditLog"/> class.
        /// </summary>
        public AuditLog()
        {
            _entities = new List<AuditEntity>();
        }

        /// <summary>
        /// Gets or sets the user name that made the changes.
        /// </summary>
        /// <value>The user name that made the changes.</value>
        [XmlAttribute("username")]
        public string Username { get; set; }

        /// <summary>
        /// Gets or sets the date when the changes were made.
        /// </summary>
        /// <value>The date when the changes were made.</value>
        [XmlAttribute("date")]
        public DateTime Date { get; set; }

        /// <summary>
        /// Gets the list entities that have changes.
        /// </summary>
        /// <value>The list entities that have changes.</value>
        [XmlElement("entity", typeof(AuditEntity))]
        public List<AuditEntity> Entities
        {
            get { return _entities; }
        }

        /// <summary>
        /// Refresh key and property values. Call after Save to capture database updated keys and values.
        /// </summary>
        public AuditLog Refresh()
        {
            // update current values because the entites can change after submit
            foreach (var auditEntity in Entities)
            {
                // don't need to update deletes
                if (auditEntity.Action == AuditAction.Deleted)
                    continue;

                // if current is stored, it will be updated on submit
                object current = auditEntity.Current;
                if (current == null)
                    continue;

                // update the key value
                foreach (var key in auditEntity.Keys)
                    key.Value = LateBinder.GetProperty(current, key.Name);

                // update the property current values
                foreach (var property in auditEntity.Properties.Where(p => !p.IsRelationship))
                    property.Current = LateBinder.GetProperty(current, property.Name);
            }

            return this;
        }

        /// <summary>
        /// Returns an XML string of the <see cref="AuditLog"/>.
        /// </summary>
        /// <returns>An XML string of the <see cref="AuditLog"/>.</returns>
        public string ToXml()
        {
            var settings = new XmlWriterSettings { Indent = true, OmitXmlDeclaration = true };
            var builder = new StringBuilder();

            using (var writer = XmlWriter.Create(builder, settings))
            {
                ToXml(writer);
            }

            return builder.ToString();
        }

        /// <summary>
        /// Saves the <see cref="AuditLog"/> to the specifed XmlWriter.
        /// </summary>
        /// <param name="writer">The writer to save <see cref="AuditLog"/> to.</param>
        public void ToXml(XmlWriter writer)
        {
            _serializer.Value.Serialize(writer, this);
            writer.Flush();
        }

        /// <summary>
        /// Returns an <see cref="AuditLog"/> object created from an XML string.
        /// </summary>
        /// <param name="auditLog">
        /// An XML string
        /// </param>
        /// <returns>
        /// An <see cref="AuditLog"/> object created from an XML string.
        /// </returns>
        public static AuditLog FromXml(string auditLog)
        {
            if (string.IsNullOrEmpty(auditLog))
                return new AuditLog();

            try
            {
                using (var reader = new StringReader(auditLog))
                using (var xr = XmlReader.Create(reader))
                {
                    return FromXml(xr);
                }
            }
            catch
            {
                return new AuditLog();
            }
        }

        /// <summary>
        /// Returns an <see cref="AuditLog"/> object created from an XML string.
        /// </summary>
        /// <param name="reader">The <see cref="XmlReader"/> to create the AuditLog from.</param>
        /// <returns>
        /// An <see cref="AuditLog"/> object created from an <see cref="XmlReader"/>.
        /// </returns>
        public static AuditLog FromXml(XmlReader reader)
        {
            if (reader == null)
                return new AuditLog();

            try
            {
                return _serializer.Value.Deserialize(reader) as AuditLog;
            }
            catch
            {
                return new AuditLog();
            }
        }
    }
}