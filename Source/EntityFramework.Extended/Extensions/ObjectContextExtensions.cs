using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Common;
using System.Data.Entity;
using System.Data.Entity.Infrastructure;
using System.Data.Metadata.Edm;
using System.Data.Objects;
using System.Linq;
using System.Text;

namespace EntityFramework.Extensions
{
    /// <summary>
    /// Extensions methods for <see cref="ObjectContext"/> and <see cref="DbContext"/>.
    /// </summary>
    public static class ObjectContextExtensions
    {
        /// <summary>
        /// Starts a database transaction from the database provider connection.
        /// </summary>
        /// <param name="context">The <see cref="ObjectContext"/> to get the database connection from.</param>
        /// <param name="isolationLevel">Specifies the isolation level for the transaction.</param>
        /// <returns>
        /// An object representing the new transaction.
        /// </returns>
        public static DbTransaction BeginTransaction(this ObjectContext context, IsolationLevel isolationLevel = IsolationLevel.Unspecified)
        {
            if (context == null)
                throw new ArgumentNullException("context");

            if (context.Connection.State != ConnectionState.Open)
                context.Connection.Open();

            return context.Connection.BeginTransaction(isolationLevel);
        }

        /// <summary>
        /// Starts a database transaction from the database provider connection.
        /// </summary>
        /// <param name="context">The <see cref="DbContext"/> to get the database connection from.</param>
        /// <param name="isolationLevel">Specifies the isolation level for the transaction.</param>
        /// <returns>
        /// An object representing the new transaction.
        /// </returns>
        public static DbTransaction BeginTransaction(this DbContext context, IsolationLevel isolationLevel = IsolationLevel.Unspecified)
        {
            if (context == null)
                throw new ArgumentNullException("context");

            var adapter = context as IObjectContextAdapter;
            return adapter.ObjectContext.BeginTransaction(isolationLevel);
        }

        internal static EntitySetBase GetEntitySet<TEntity>(this ObjectContext context)
        {
            string name = typeof(TEntity).FullName;
            return GetEntitySet(context, name);
        }

        internal static EntitySetBase GetEntitySet(this ObjectContext context, string elementTypeName)
        {
            var container = context.MetadataWorkspace.GetEntityContainer(context.DefaultContainerName, DataSpace.CSpace);
            return container.BaseEntitySets.FirstOrDefault(item => item.ElementType.FullName.Equals(elementTypeName));
        }

    }
}
