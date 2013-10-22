using System;
using System.Data.Entity.Core.Objects;

namespace EntityFramework.Future
{
    /// <summary>
    /// The command plan for a future query.
    /// </summary>
    public class FuturePlan
    {
        /// <summary>
        /// Gets or sets the command text.
        /// </summary>
        /// <value>
        /// The command text.
        /// </value>
        public string CommandText { get; set; }
        /// <summary>
        /// Gets or sets the parameters.
        /// </summary>
        /// <value>
        /// The parameters.
        /// </value>
        public ObjectParameterCollection Parameters { get; set; }
    }
}