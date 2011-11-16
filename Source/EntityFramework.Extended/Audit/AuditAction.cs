namespace EntityFramework.Audit
{
    /// <summary>
    /// A list of entity actions for the audit log.
    /// </summary>
    public enum AuditAction
    {
        /// <summary>
        /// The entity was inserted/added.
        /// </summary>
        Added = 1,
        /// <summary>
        /// The entity was updated/modifed.
        /// </summary>
        Modified = 2,
        /// <summary>
        /// The entity was deleted.
        /// </summary>
        Deleted = 3
    }
}