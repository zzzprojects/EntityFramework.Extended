using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Common;
using System.Linq;
using System.Text;

namespace EntityFramework.Extensions
{
    public static class DataRecordExtensions
    {
        public static object GetValue(this IDataRecord record, string name)
        {
            int ordinal = record.GetOrdinal(name);
            if (record.IsDBNull(ordinal))
                return null;

            return record.GetValue(ordinal);
        }
    }
}
