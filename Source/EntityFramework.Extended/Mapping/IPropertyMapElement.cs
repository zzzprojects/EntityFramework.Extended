using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EntityFramework.Mapping
{
    /// <summary>
    /// Interface representing a property map element which can be single or complex
    /// </summary>
    public interface IPropertyMapElement
    {
        /// <summary>
        /// Gets or sets the name of the property.
        /// </summary>
        string PropertyName { get; set; }
    }
}
