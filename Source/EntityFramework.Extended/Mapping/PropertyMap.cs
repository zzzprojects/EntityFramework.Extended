using System.Diagnostics;

namespace EntityFramework.Mapping
{
    [DebuggerDisplay("Property: {PropertyName}, Column: {ColumnName}")]
    public class PropertyMap
    {
        public string PropertyName { get; set; }
        public string ColumnName { get; set; }
    }
}