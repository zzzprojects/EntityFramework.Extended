using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Data.Entity.Core.Objects;
using System.Linq;
using System.Text;

namespace EntityFramework.Mapping
{
    /// <summary>
    /// An <see langword="interface"/> defining a provider to get entity mapping data.
    /// </summary>
    public interface IMappingProvider
    {
        /// <summary>
        /// Gets the <see cref="EntityMap"/> for the specified <typeparamref name="TEntity"/>.
        /// </summary>
        /// <typeparam name="TEntity">The type of the entity.</typeparam>
        /// <param name="query">The query to use to help load the mapping data.</param>
        /// <returns>An <see cref="EntityMap"/> with the mapping data.</returns>
        EntityMap GetEntityMap<TEntity>(ObjectQuery query);

        /// <summary>
        /// Gets the <see cref="EntityMap"/> for the specified <paramref name="type"/>.
        /// </summary>
        /// <param name="type">The type of the entity.</param>
        /// <param name="dbContext">The database context to load metadata from.</param>
        /// <returns>An <see cref="EntityMap"/> with the mapping data.</returns>
        EntityMap GetEntityMap(Type type, DbContext dbContext);
        
        /// <summary>
        /// Gets the <see cref="EntityMap"/> for the specified <paramref name="type"/>.
        /// </summary>
        /// <param name="type">The type of the entity.</param>
        /// <param name="objectContext">The object context to load metadata from.</param>
        /// <returns>An <see cref="EntityMap"/> with the mapping data.</returns>
        EntityMap GetEntityMap(Type type, ObjectContext objectContext);
    }
}
