using System.Diagnostics;

namespace EntityFramework.Mapping
{
    /// <summary>
    /// A class representing a property map
    /// </summary>
    [DebuggerDisplay("Property: {PropertyName}, Column: {ColumnName}")]
    public class PropertyMap : IPropertyMapElement
    {
        /// <summary>
        /// Create a new instance.
        /// </summary>
        /// <param name="propertyName">the property name</param>
        /// <param name="columnName">the column name</param>
        public PropertyMap(string propertyName, string columnName)
        {
            ColumnName = columnName;
            PropertyName = propertyName;
        }

        /// <summary>
        /// Gets or sets the name of the property.
        /// </summary>
        public string PropertyName { get; private set; }

        /// <summary>
        /// Gets or sets the name of the column.
        /// </summary>
        public string ColumnName { get; private set; }
    }
}