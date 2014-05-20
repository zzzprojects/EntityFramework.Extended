using System.Collections.Generic;
using System.Linq;

namespace EntityFramework.Mapping
{
    /// <summary>
    /// A property map element representing a complex class
    /// </summary>
    public class ComplexPropertyMap : IPropertyMapElement
    {
        private readonly IList<IPropertyMapElement> _elements;

        /// <summary>
        /// Create a new instance.
        /// </summary>
        /// <param name="propertyName">the property name.</param>
        /// <param name="elements">the mapped elements of the complex property</param>
        public ComplexPropertyMap(string propertyName, IEnumerable<IPropertyMapElement> elements)
        {
            _elements = elements.ToList();
            PropertyName = propertyName;
        }

        /// <summary>
        /// The property name.
        /// </summary>
        public string PropertyName { get; private set; }

        /// <summary>
        /// The complex type's elements.
        /// </summary>
        public IEnumerable<IPropertyMapElement> Elements
        {
            get { return _elements; }
        }
    }
}
