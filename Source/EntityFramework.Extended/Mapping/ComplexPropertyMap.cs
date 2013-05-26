using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EntityFramework.Mapping
{
    /// <summary>
    /// A property map element representing a complex class
    /// </summary>
    public class ComplexPropertyMap: IPropertyMapElement
    {

        public string PropertyName { get; set; }

        /// <summary>
        /// The enumeration of the complex' type 
        /// </summary>
        public ICollection<IPropertyMapElement> TypeElements { get; set; }
    }
}
