using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Data.Entity.Core.Mapping;
using System.Data.Entity.Core.Metadata.Edm;
using System.Data.Entity.Core.Objects;
using System.Data.Entity.Infrastructure;
using System.Linq;
using System.Text;

namespace EntityFramework.Mapping
{
    /// <summary>
    ///     Use <see cref="MetadataWorkspace" /> to resolve mapping information.
    /// </summary>
    public class MetadataMappingProvider : IMappingProvider
    {
        /// <summary>
        ///     Gets the <see cref="EntityMap" /> for the specified <typeparamref name="TEntity" />.
        /// </summary>
        /// <typeparam name="TEntity">The type of the entity.</typeparam>
        /// <param name="query">The query to use to help load the mapping data.</param>
        /// <returns>
        ///     An <see cref="EntityMap" /> with the mapping data.
        /// </returns>
        public EntityMap GetEntityMap<TEntity>(ObjectQuery query)
        {
            var context = query.Context;
            var type = typeof(TEntity);

            return GetEntityMap(type, context);
        }

        /// <summary>
        ///     Gets the <see cref="EntityMap" /> for the specified <paramref name="type" />.
        /// </summary>
        /// <param name="type">The type of the entity.</param>
        /// <param name="dbContext">The database context to load metadata from.</param>
        /// <returns>
        ///     An <see cref="EntityMap" /> with the mapping data.
        /// </returns>
        public EntityMap GetEntityMap(Type type, DbContext dbContext)
        {
            var objectContextAdapter = dbContext as IObjectContextAdapter;
            var objectContext = objectContextAdapter.ObjectContext;
            return GetEntityMap(type, objectContext);
        }

        /// <summary>
        ///     Gets the <see cref="EntityMap" /> for the specified <paramref name="type" />.
        /// </summary>
        /// <param name="type">The type of the entity.</param>
        /// <param name="objectContext">The object context to load metadata from.</param>
        /// <returns>
        ///     An <see cref="EntityMap" /> with the mapping data.
        /// </returns>
        public EntityMap GetEntityMap(Type type, ObjectContext objectContext)
        {
            var entityMap = new EntityMap(type);
            var metadata = objectContext.MetadataWorkspace;

            var objectItemCollection = ((ObjectItemCollection)metadata.GetItemCollection(DataSpace.OSpace));
            var entityType = metadata.GetItems<EntityType>(DataSpace.OSpace).Single(e => objectItemCollection.GetClrType(e) == type);

            var entitySet = metadata.GetItems<EntityContainer>(DataSpace.CSpace)
                .SelectMany(a => a.EntitySets).FirstOrDefault(s => s.ElementType.Name == entityType.Name);

            var entitySetMappings = metadata.GetItems<EntityContainerMapping>(DataSpace.CSSpace).Single().EntitySetMappings.ToList();
            var mapping = GetMapping(entitySetMappings, metadata.GetItems(DataSpace.CSpace)
                .Where(x => x.BuiltInTypeKind == BuiltInTypeKind.EntityType)
                .Cast<EntityType>()
                .Single(x => x.Name == entityType.Name));

            // Find the storage entity set (table) that the entity is mapped
            var mappingFragment =
                (mapping.EntityTypeMappings.FirstOrDefault(a => a.IsHierarchyMapping) ??
                 mapping.EntityTypeMappings.First()).Fragments.First();

            entityMap.ModelType = entityType;
            entityMap.ModelSet = entitySet;
            entityMap.StoreSet = mappingFragment.StoreEntitySet;
            entityMap.StoreType = mappingFragment.StoreEntitySet.ElementType;

            // set table
            SetTableName(entityMap);

            // set properties
            SetProperties(entityMap, mapping, type);

            // set keys
            SetKeys(entityMap);

            return entityMap;
        }

        private static EntitySetMapping GetMapping(List<EntitySetMapping> entitySetMappings, EntityType entitySet)
        {
            var mapping = entitySetMappings.SingleOrDefault(x => x.EntitySet.Name == entitySet.Name);
            if (mapping != null)
            {
                return mapping;
            }
            mapping = entitySetMappings.SingleOrDefault(
                x => x.EntityTypeMappings.Where(y => y.EntityType != null).Any(y => y.EntityType.Name == entitySet.Name));
            if (mapping != null)
            {
                return mapping;
            }
            return entitySetMappings.Single(x => x.EntityTypeMappings.Any(y => y.IsOfEntityTypes.Any(z => z.Name == entitySet.Name)));
        }

        private static void SetKeys(EntityMap entityMap)
        {
            var modelType = entityMap.ModelType;
            foreach (var edmMember in modelType.KeyMembers)
            {
                var property = entityMap.PropertyMaps.FirstOrDefault(p => p.PropertyName == edmMember.Name);
                if (property == null)
                {
                    continue;
                }

                var map = new PropertyMap
                {
                    PropertyName = property.PropertyName,
                    ColumnName = property.ColumnName
                };
                entityMap.KeyMaps.Add(map);
            }
        }

        private static IEnumerable<Type> GetParentTypes(Type type)
        {
            // is there any base type?
            if ((type == null) || (type.BaseType == null))
            {
                yield break;
            }

            // return all implemented or inherited interfaces
            foreach (var i in type.GetInterfaces())
            {
                yield return i;
            }

            // return all inherited types
            var currentBaseType = type.BaseType;
            while (currentBaseType != null)
            {
                yield return currentBaseType;
                currentBaseType = currentBaseType.BaseType;
            }
        }

        private static void SetProperties(EntityMap entityMap, EntitySetMapping mapping, Type type)
        {
            var isTypeOf = new HashSet<string>(GetParentTypes(type).Union(new[] {type}).Select(o => o.Name));

            foreach (var propertyMapping in
                mapping.EntityTypeMappings.Where(
                    o => o.EntityTypes == null || o.EntityTypes.Count < 1 ||
                         o.EntityTypes.Any(et => isTypeOf.Contains(et.Name)))
                    .SelectMany(o => o.Fragments)
                    .SelectMany(o => o.PropertyMappings)
                    //.Where(o => o.Property.DeclaringType.)
                    .GroupBy(o => o.Property.Name).Select(o => o.First()))
            {
                var map = new PropertyMap
                {
                    PropertyName = propertyMapping.Property.Name
                };

                entityMap.PropertyMaps.Add(map);

                var scalarPropertyMapping = propertyMapping as ScalarPropertyMapping;
                if (scalarPropertyMapping != null)
                {
                    map.ColumnName = scalarPropertyMapping.Column.Name;
                    continue;
                }

                // TODO support complex mapping
                var complexPropertyMapping = propertyMapping as ComplexPropertyMapping;
            }
        }

        private static void SetTableName(EntityMap entityMap)
        {
            var builder = new StringBuilder(50);

            EntitySet storeSet = entityMap.StoreSet;

            string table = null;
            string schema = null;

            MetadataProperty tableProperty;
            MetadataProperty schemaProperty;

            storeSet.MetadataProperties.TryGetValue("Table", true, out tableProperty);
            if (tableProperty == null || tableProperty.Value == null)
            {
                storeSet.MetadataProperties.TryGetValue("http://schemas.microsoft.com/ado/2007/12/edm/EntityStoreSchemaGenerator:Table",
                    true, out tableProperty);
            }

            if (tableProperty != null)
            {
                table = tableProperty.Value as string;
            }

            // Table will be null if its the same as Name
            if (table == null)
            {
                table = storeSet.Name;
            }

            storeSet.MetadataProperties.TryGetValue("Schema", true, out schemaProperty);
            if (schemaProperty == null || schemaProperty.Value == null)
            {
                storeSet.MetadataProperties.TryGetValue("http://schemas.microsoft.com/ado/2007/12/edm/EntityStoreSchemaGenerator:Schema",
                    true, out schemaProperty);
            }

            if (schemaProperty != null)
            {
                schema = schemaProperty.Value as string;
            }

            if (!string.IsNullOrWhiteSpace(schema))
            {
                builder.Append(QuoteIdentifier(schema));
                builder.Append(".");
            }

            builder.Append(QuoteIdentifier(table));

            entityMap.TableName = builder.ToString();
        }

        private static string QuoteIdentifier(string name)
        {
            return ("[" + name.Replace("]", "]]") + "]");
        }
    }
}