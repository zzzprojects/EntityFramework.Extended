using System;
using System.Collections.Generic;
using System.Data.Metadata.Edm;
using System.Data.Objects;
using System.Linq;
using System.Text;
using EntityFramework.Reflection;

namespace EntityFramework.Mapping
{
    /// <summary>
    /// A provider class to get entity mapping data.
    /// </summary>
    /// <remarks>
    /// <para>
    /// This class uses a lot of reflection to get access to internal mapping data that Entity Framework
    /// does not provide access to.  There maybe issues with this implementation as version of of Entity Framework change.
    /// </para>
    /// <para>
    /// This implementation can be overridden using the <see cref="Locator"/> container resolving 
    /// the <see cref="IMappingProvider"/> <see langword="interface"/>.
    /// </para>
    /// </remarks>
    public class ReflectionMappingProvider : IMappingProvider
    {
        /// <summary>
        /// Gets the <see cref="EntityMap"/> for the specified <typeparamref name="TEntity"/>.
        /// </summary>
        /// <typeparam name="TEntity">The type of the entity.</typeparam>
        /// <param name="query">The query to use to help load the mapping data.</param>
        /// <returns>
        /// An <see cref="EntityMap"/> with the mapping data.
        /// </returns>
        public EntityMap GetEntityMap<TEntity>(ObjectQuery query)
        {
            return CreateEntityMap<TEntity>(query);
        }

        private static EntityMap CreateEntityMap<TEntity>(ObjectQuery query)
        {
            Type entityType = typeof(TEntity);
            ObjectContext objectContext = query.Context;

            var modelSet = objectContext.GetEntitySet<TEntity>();
            if (modelSet == null)
                return null;

            var entityMap = new EntityMap(entityType);
            entityMap.ModelSet = modelSet;
            entityMap.ModelType = entityMap.ModelSet.ElementType;

            var metadata = objectContext.MetadataWorkspace;

            // force metadata to load
            dynamic dbProxy = new DynamicProxy(objectContext);
            dbProxy.EnsureMetadata();

            ItemCollection itemCollection;
            if (!metadata.TryGetItemCollection(DataSpace.CSSpace, out itemCollection))
            {
                // force CSSpace to load
                query.ToTraceString();
                // try again
                metadata.TryGetItemCollection(DataSpace.CSSpace, out itemCollection);
            }

            if (itemCollection == null)
                return null;

            dynamic mappingFragmentProxy = FindMappingFragment(itemCollection, entityMap.ModelSet);
            if (mappingFragmentProxy == null)
                return null;

            // SModel
            entityMap.StoreSet = mappingFragmentProxy.TableSet;
            entityMap.StoreType = entityMap.StoreSet.ElementType;

            SetProperties(entityMap, mappingFragmentProxy);
            SetKeys(entityMap);
            SetTableName(entityMap);

            return entityMap;
        }

        private static dynamic FindMappingFragment(IEnumerable<GlobalItem> itemCollection, EntitySet entitySet)
        {
            //StorageEntityContainerMapping
            var storage = itemCollection.FirstOrDefault();
            if (storage == null)
                return null;

            dynamic storageProxy = new DynamicProxy(storage);

            //StorageSetMapping
            dynamic mappings = storageProxy.EntitySetMaps;
            if (mappings == null)
                return null;

            foreach (object mapping in mappings)
            {
                dynamic mappingProxy = new DynamicProxy(mapping);
                EntitySet modelSet = mappingProxy.Set;
                if (modelSet == null || modelSet != entitySet)
                    continue;

                // only support first type mapping
                IEnumerable<object> typeMappings = mappingProxy.TypeMappings;
                if (typeMappings == null)
                    continue;

                object typeMapping = typeMappings.FirstOrDefault();
                if (typeMapping == null)
                    continue;

                // StorageEntityTypeMapping
                dynamic typeMappingProxy = new DynamicProxy(typeMapping);

                // only support first mapping fragment
                IEnumerable<object> mappingFragments = typeMappingProxy.MappingFragments;
                if (mappingFragments == null)
                    continue;
                object mappingFragment = mappingFragments.FirstOrDefault();
                if (mappingFragment == null)
                    continue;

                //StorageMappingFragment
                dynamic mappingFragmentProxy = new DynamicProxy(mappingFragment);
                return mappingFragmentProxy;
            }

            return null;
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

        private static void SetProperties(EntityMap entityMap, dynamic mappingFragmentProxy)
        {
            var propertyMaps = mappingFragmentProxy.Properties;
            foreach (var propertyMap in propertyMaps)
            {
                // StorageScalarPropertyMapping
                dynamic propertyMapProxy = new DynamicProxy(propertyMap);

                EdmProperty modelProperty = propertyMapProxy.EdmProperty;
                EdmProperty storeProperty = propertyMapProxy.ColumnProperty;

                var map = new PropertyMap
                {
                    ColumnName = storeProperty.Name,
                    PropertyName = modelProperty.Name
                };

                entityMap.PropertyMaps.Add(map);
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
