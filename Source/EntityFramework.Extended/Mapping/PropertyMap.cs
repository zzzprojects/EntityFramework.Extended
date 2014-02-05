using System.Diagnostics;

namespace EntityFramework.Mapping
{
    /// <summary>
    /// A class representing a property map
    /// </summary>
    [DebuggerDisplay("Property: {PropertyName}, Column: {ColumnName}")]
    public class PropertyMap
    {
        /// <summary>
        /// Gets or sets the name of the property.
        /// </summary>
        public string PropertyName { get; set; }
        /// <summary>
        /// Gets or sets the name of the column.
        /// </summary>
        public string ColumnName { get; set; }
    }
    /// <summary>
    /// A class representing a property map for a constant value
    /// </summary>
    [DebuggerDisplay("Property: {PropertyName}, Column: {ColumnName}, Value: {Value}")]
    public class ConstantPropertyMap : PropertyMap
    {
        /// <summary>
        /// Gets or sets the constant value
        /// </summary>
        public object Value { get; set; }
    }
}
