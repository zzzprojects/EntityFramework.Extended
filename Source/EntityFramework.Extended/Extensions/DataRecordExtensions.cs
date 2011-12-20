using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Common;
using System.Linq;
using System.Text;

namespace EntityFramework.Extensions
{
    /// <summary>
    /// Extension methods for IDataRecord.
    /// </summary>
    public static class DataRecordExtensions
    {
        /// <summary>
        /// Gets the value of the specified field name.
        /// </summary>
        /// <param name="record">The IDataRecord to read.</param>
        /// <param name="name">The field name.</param>
        /// <returns></returns>
        public static object GetValue(this IDataRecord record, string name)
        {
            int ordinal = record.GetOrdinal(name);
            if (record.IsDBNull(ordinal))
                return null;

            return record.GetValue(ordinal);
        }
    }
}
