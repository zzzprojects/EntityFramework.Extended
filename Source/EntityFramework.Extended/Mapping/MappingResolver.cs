using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Data.Metadata.Edm;
using System.Data.Objects;
using System.Linq;
using System.Text;
using EntityFramework.Extensions;
using EntityFramework.Reflection;

namespace EntityFramework.Mapping
{
    public static class MappingResolver
    {
        private static readonly ConcurrentDictionary<string, Dictionary<Type, EntitySet>> _entitySetMappings;
        private static readonly ConcurrentDictionary<Type, EntityMap> _entityMapping;

        static MappingResolver()
        {
            _entitySetMappings = new ConcurrentDictionary<string, Dictionary<Type, EntitySet>>();
            _entityMapping = new ConcurrentDictionary<Type, EntityMap>();
        }

        public static EntityMap GetEntityMap<TEntity>(this ObjectQuery query)
        {
            return _entityMapping.GetOrAdd(
              typeof(TEntity),
              k => CreateEntityMap<TEntity>(query));
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

            string schema = null;
            MetadataProperty schemaProperty;

            storeSet.MetadataProperties.TryGetValue("Schema", true, out schemaProperty);
            if (schemaProperty == null)
                storeSet.MetadataProperties.TryGetValue("http://schemas.microsoft.com/ado/2007/12/edm/EntityStoreSchemaGenerator:Schema", true, out schemaProperty);

            if (schemaProperty != null)
                schema = schemaProperty.Value as string;

            if (!string.IsNullOrWhiteSpace(schema))
            {
                builder.Append(QuoteIdentifier(schema));
                builder.Append(".");
            }

            builder.Append(QuoteIdentifier(storeSet.Name));

            entityMap.TableName = builder.ToString();
        }

        private static string QuoteIdentifier(string name)
        {
            return ("[" + name.Replace("]", "]]") + "]");
        }

        public static EntitySet GetEntitySet<TEntity>(this ObjectContext objectContext)
        {
            var entityType = typeof (TEntity);
            var mapping = _entitySetMappings.GetOrAdd(objectContext.DefaultContainerName, k =>
            {
                var metadataWorkspace = objectContext.MetadataWorkspace;
                // make sure types are loaded
                Type baseType = entityType;
                do
                {
                    metadataWorkspace.LoadFromAssembly(baseType.Assembly);
                    baseType = baseType.BaseType;
                } while ((baseType != null) && (baseType != typeof(object)));

                return CreateEntitySetMappings(metadataWorkspace);
            });

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
