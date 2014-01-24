using System.Runtime.Serialization;

namespace EntityFramework.Audit
{
    /// <summary>
    /// A list of entity actions for the audit log.
    /// </summary>
    [DataContract(Name = "action", Namespace = AuditLog.AuditNamespace)]
    public enum AuditAction
    {
        /// <summary>
        /// The entity was inserted/added.
        /// </summary>
        [EnumMember]
        Added = 1,
        /// <summary>
        /// The entity was updated/modifed.
        /// </summary>
        [EnumMember]
        Modified = 2,
        /// <summary>
        /// The entity was deleted.
        /// </summary>
        [EnumMember]
        Deleted = 3
    }
}