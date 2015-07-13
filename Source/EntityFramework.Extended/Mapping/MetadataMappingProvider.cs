using System;
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
    /// Use <see cref="MetadataWorkspace"/> to resolve mapping information.
    /// </summary>
    public class MetadataMappingProvider : IMappingProvider
    {
        /// <summary>
        /// Gets the <see cref="EntityMap" /> for the specified <typeparamref name="TEntity" />.
        /// </summary>
        /// <typeparam name="TEntity">The type of the entity.</typeparam>
        /// <param name="query">The query to use to help load the mapping data.</param>
        /// <returns>
        /// An <see cref="EntityMap" /> with the mapping data.
        /// </returns>
        public EntityMap GetEntityMap<TEntity>(ObjectQuery query)
        {
            var context = query.Context;
            var type = typeof(TEntity);

            return GetEntityMap(type, context);
        }

        /// <summary>
        /// Gets the <see cref="EntityMap" /> for the specified <paramref name="type" />.
        /// </summary>
        /// <param name="type">The type of the entity.</param>
        /// <param name="dbContext">The database context to load metadata from.</param>
        /// <returns>
        /// An <see cref="EntityMap" /> with the mapping data.
        /// </returns>
        public EntityMap GetEntityMap(Type type, DbContext dbContext)
        {
            var objectContextAdapter = dbContext as IObjectContextAdapter;
            var objectContext = objectContextAdapter.ObjectContext;
            return GetEntityMap(type, objectContext);
        }

        /// <summary>
        /// Gets the <see cref="EntityMap" /> for the specified <paramref name="type" />.
        /// </summary>
        /// <param name="type">The type of the entity.</param>
        /// <param name="objectContext">The object context to load metadata from.</param>
        /// <returns>
        /// An <see cref="EntityMap" /> with the mapping data.
        /// </returns>
        public EntityMap GetEntityMap(Type type, ObjectContext objectContext)
        {
            var entityMap = new EntityMap(type);
            var metadata = objectContext.MetadataWorkspace;

            // Get the part of the model that contains info about the actual CLR types
            var objectItemCollection = ((ObjectItemCollection)metadata.GetItemCollection(DataSpace.OSpace));

            // Get the entity type from the model that maps to the CLR type
            var entityType = metadata
                    .GetItems<EntityType>(DataSpace.OSpace)
                    .First(e => objectItemCollection.GetClrType(e) == type);

            // Get the entity set that uses this entity type
            var entitySet = metadata.GetItems<EntityContainer>(DataSpace.CSpace)
                                    .SelectMany(a => a.EntitySets)
                                    .Where(s => s.ElementType.Name == entityType.Name)
                                    .FirstOrDefault();


            // Find the mapping between conceptual and storage model for this entity set
            var mapping = metadata.GetItems<EntityContainerMapping>(DataSpace.CSSpace)
                                    .SelectMany(a => a.EntitySetMappings)
                                    .First(s => s.EntitySet == entitySet);

            // Find the storage entity set (table) that the entity is mapped
            var mappingFragment = (mapping.EntityTypeMappings.FirstOrDefault(a => a.IsHierarchyMapping) ?? mapping.EntityTypeMappings.First()).Fragments.First();

            entityMap.ModelType = entityType;
            entityMap.ModelSet = entitySet;
            entityMap.StoreSet = mappingFragment.StoreEntitySet;
            entityMap.StoreType = mappingFragment.StoreEntitySet.ElementType;

            // set table
            SetTableName(entityMap);

            // set properties
            SetProperties(entityMap, mappingFragment);

            // set keys
            SetKeys(entityMap);

            return entityMap;
        }

        private static void SetKeys(EntityMap entityMap)
        {
            var modelType = entityMap.ModelType;
            foreach (var edmMember in modelType.KeyMembers)
            {
                var property = entityMap.PropertyMaps.FirstOrDefault(p => p.PropertyName == edmMember.Name);
                if (property == null)
                    continue;

                var map = new PropertyMap
                {
                    PropertyName = property.PropertyName,
                    ColumnName = property.ColumnName
                };
                entityMap.KeyMaps.Add(map);
            }
        }

        private static void SetProperties(EntityMap entityMap, MappingFragment mappingFragment)
        {
            foreach (var propertyMapping in mappingFragment.PropertyMappings)
            {
                var map = new PropertyMap();
                map.PropertyName = propertyMapping.Property.Name;

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
            EntitySet storeSet = entityMap.StoreSet;

            string table = null;
            string schema = null;

            MetadataProperty tableProperty;
            MetadataProperty schemaProperty;

            storeSet.MetadataProperties.TryGetValue("Table", true, out tableProperty);
            if (tableProperty == null || tableProperty.Value == null)
                storeSet.MetadataProperties.TryGetValue("http://schemas.microsoft.com/ado/2007/12/edm/EntityStoreSchemaGenerator:Table", true, out tableProperty);

            if (tableProperty != null)
                table = tableProperty.Value as string;

            // Table will be null if its the same as Name
            if (table == null)
                table = storeSet.Name;

            storeSet.MetadataProperties.TryGetValue("Schema", true, out schemaProperty);
            if (schemaProperty == null || schemaProperty.Value == null)
                storeSet.MetadataProperties.TryGetValue("http://schemas.microsoft.com/ado/2007/12/edm/EntityStoreSchemaGenerator:Schema", true, out schemaProperty);

            if (schemaProperty != null)
                schema = schemaProperty.Value as string;

            entityMap.SchemaName = schema;
            entityMap.TableName = table;
        }


    }
}
