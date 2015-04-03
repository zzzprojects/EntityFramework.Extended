using System.Collections.ObjectModel;
using System.Runtime.Serialization;

namespace EntityFramework.Audit
{
    /// <summary>
    /// A keyed collection of <see cref="AuditProperty"/>
    /// </summary>
    [CollectionDataContract(Name = "properties", ItemName = "property", Namespace = AuditLog.AuditNamespace)]
    public class AuditPropertyCollection : KeyedCollection<string, AuditProperty>
    {
        /// <summary>
        /// Extracts the key from the specified element.
        /// </summary>
        /// <param name="item">The element from which to extract the key.</param>
        /// <returns>The key for the specified element.</returns>
        protected override string GetKeyForItem(AuditProperty item)
        {
            return item.Name;
        }
    }
}
