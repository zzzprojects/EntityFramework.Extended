using System.Collections.ObjectModel;
using System.Runtime.Serialization;

namespace EntityFramework.Audit
{
    /// <summary>
    /// A keyed collection of <see cref="AuditKey"/>
    /// </summary>
    [CollectionDataContract(Name = "keys", ItemName = "key", Namespace = AuditLog.AuditNamespace)]
    public class AuditKeyCollection : KeyedCollection<string, AuditKey>
    {
        /// <summary>
        /// When implemented in a derived class, extracts the key from the specified element.
        /// </summary>
        /// <param name="item">The element from which to extract the key.</param>
        /// <returns>The key for the specified element.</returns>
        protected override string GetKeyForItem(AuditKey item)
        {
            return item.Name;
        }
    }
}
