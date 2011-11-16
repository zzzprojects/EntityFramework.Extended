using System.Collections.ObjectModel;

namespace EntityFramework.Audit
{
    /// <summary>
    /// A keyed collection of <see cref="AuditKey"/>
    /// </summary>
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
