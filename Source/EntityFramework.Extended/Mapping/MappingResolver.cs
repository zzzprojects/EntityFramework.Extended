using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Data.Entity.Core.Metadata.Edm;
using System.Data.Entity.Core.Objects;
using System.Linq;

namespace EntityFramework.Mapping
{
    /// <summary>
    /// Static class to resolve Entity Framework mapping.
    /// </summary>
    public static class MappingResolver
    {
        static MappingResolver()
        {
        }

        /// <summary>
        /// Gets an <see cref="EntityMap"/> for the entity type used in the specified <paramref name="query"/>.
        /// </summary>
        /// <typeparam name="TEntity">The type of the entity.</typeparam>
        /// <param name="query">The query used to create the <see cref="EntityMap"/> from.</param>
        /// <returns>An <see cref="EntityMap"/> for the specified <paramref name="query"/>.</returns>
        public static EntityMap GetEntityMap<TEntity>(this ObjectQuery query)
        {
            var provider = Locator.Current.Resolve<IMappingProvider>();
            return provider.GetEntityMap<TEntity>(query);
        }

        /// <summary>
        /// Gets the underling <see cref="EntitySet"/> for the type of entity.
        /// </summary>
        /// <typeparam name="TEntity">The type of the entity.</typeparam>
        /// <param name="objectContext">The <see cref="ObjectContext"/> context to get the <see cref="EntitySet"/> from.</param>
        /// <returns>The <see cref="EntitySet"/> for the type of entity specified.</returns>
        public static EntitySet GetEntitySet<TEntity>(this ObjectContext objectContext)
        {
            var entityType = typeof(TEntity);
            var metadataWorkspace = objectContext.MetadataWorkspace;
            
            // make sure types are loaded
            Type baseType = entityType;
            do
            {
                metadataWorkspace.LoadFromAssembly(baseType.Assembly);
                baseType = baseType.BaseType;
            } while ((baseType != null) && (baseType != typeof(object)));

            var mapping = CreateEntitySetMappings(metadataWorkspace);
            EntitySet entitySet;
            mapping.TryGetValue(entityType, out entitySet);
            return entitySet;
        }

        private static Dictionary<Type, EntitySet> CreateEntitySetMappings(MetadataWorkspace metadataWorkspace)
        {
            var entitySetMappings = new Dictionary<Type, EntitySet>();
            var itemCollection = (ObjectItemCollection)metadataWorkspace.GetItemCollection(DataSpace.OSpace);
            var entityTypes = metadataWorkspace.GetItems<EntityType>(DataSpace.OSpace);
            var entityContainers = metadataWorkspace.GetItems<EntityContainer>(DataSpace.CSpace);
            var stack = new Stack<EntityType>();

            if (entityTypes == null || entityContainers == null)
                return entitySetMappings;

            foreach (EntityType type in entityTypes)
            {
                Func<EntitySetBase, bool> predicate = null;
                stack.Clear();
                var cspaceType = (EntityType)metadataWorkspace.GetEdmSpaceType(type);
                do
                {
                    stack.Push(cspaceType);
                    cspaceType = (EntityType)cspaceType.BaseType;
                } while (cspaceType != null);

                EntitySet entitySet = null;
                while ((entitySet == null) && (stack.Count > 0))
                {
                    cspaceType = stack.Pop();
                    foreach (EntityContainer container in entityContainers)
                    {
                        if (predicate == null)
                            predicate = s => s.ElementType == cspaceType;

                        var source = container.BaseEntitySets
                            .Where(predicate)
                            .ToList();

                        int count = source.Count();
                        if ((count > 1) || ((count == 1) && (entitySet != null)))
                            throw new InvalidOperationException("Multiple entity sets per type is not supported.");
                        if (count == 1)
                            entitySet = (EntitySet)source.First();
                    }
                }

                if (entitySet == null)
                    continue;

                Type clrType = itemCollection.GetClrType(type);
                entitySetMappings[clrType] = entitySet;
            }

            return entitySetMappings;
        }
    }
}
