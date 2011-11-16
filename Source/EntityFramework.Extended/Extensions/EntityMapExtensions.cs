using System;
using System.Collections.Generic;
using System.Collections.Concurrent;
using System.Data.Metadata.Edm;
using System.Data.Objects;
using System.Diagnostics;
using System.Linq;
using System.Text;
using EntityFramework.Reflection;

namespace EntityFramework.Extended
{
    public static class EntityMapExtensions
    {
        private static readonly ConcurrentDictionary<Type, EntityMap> _mapCache = new ConcurrentDictionary<Type, EntityMap>();

        /// <summary>
        /// Gets entity to database mapping information.
        /// </summary>
        /// <typeparam name="TEntity">The type of the entity.</typeparam>
        /// <param name="query">The query to use as a base to extract the mapping information..</param>
        /// <returns>An EntityMap class with the mapping information,</returns>
        /// <remarks>
        /// Since there is no public way to get mapping, this is a hack to get the entity to table
        /// mapping from the metadata and is highly subject to breaking on update to the
        /// Entity Frame work internals.
        /// </remarks>
        public static EntityMap GetEntityMap<TEntity>(this ObjectQuery query)
        {
            return _mapCache.GetOrAdd(
              typeof(TEntity),
              k => CreateEntityMap<TEntity>(query));
        }

        private static EntityMap CreateEntityMap<TEntity>(ObjectQuery query)
        {
            var entityMap = new EntityMap(typeof(TEntity));

            // get execution plan
            dynamic queryProxy = new DynamicProxy(query);
            dynamic queryState = queryProxy.QueryState;
            if (queryState == null)
                return null;

            dynamic executionPlan = queryState.GetExecutionPlan(null);
            if (executionPlan == null)
                return null;

            dynamic commandDefinition = executionPlan.CommandDefinition;
            if (commandDefinition == null)
                return null;

            dynamic sets = commandDefinition.EntitySets;
            if (sets == null)
                return null;

            IEnumerable<EntitySet> entitySets = sets;

            // find mapping entity sets
            foreach (EntitySet entitySet in entitySets)
            {
                EntityType elementType = entitySet.ElementType;

                if (elementType == null || !elementType.MetadataProperties.Contains("DataSpace"))
                    continue;

                var value = elementType.MetadataProperties["DataSpace"].Value;
                if (value == null || value.GetType() != typeof(DataSpace))
                    continue;

                var dataSpace = (DataSpace)value;

                if (dataSpace == DataSpace.CSpace)
                {
                    entityMap.ModelSet = entitySet;
                    entityMap.ModelType = elementType;
                }
                else if (dataSpace == DataSpace.SSpace)
                {
                    entityMap.StoreSet = entitySet;
                    entityMap.StoreType = elementType;
                }
            } // foreach set

            // make sure we found the entity sets
            if (entityMap.ModelSet == null || entityMap.StoreSet == null)
                return null;

            // create property map
            SetKeys(entityMap);
            SetProperties(entityMap);

            // get table name
            SetTableName(entityMap);
            return entityMap;
        }

        private static void SetKeys(EntityMap entityMap)
        {
            var storeType = entityMap.StoreType;
            var modelType = entityMap.ModelType;

            int count = Math.Min(
              storeType.KeyMembers.Count,
              modelType.KeyMembers.Count);

            for (int i = 0; i < count; i++)
            {
                // assume properties are in same order
                var store = storeType.KeyMembers[i];
                var model = modelType.KeyMembers[i];

                var map = new PropertyMap
                {
                    PropertyName = model.Name,
                    ColumnName = QuoteIdentifier(store.Name)
                };
                entityMap.KeyMaps.Add(map);
            }
        }

        private static void SetProperties(EntityMap entityMap)
        {
            var storeType = entityMap.StoreType;
            var modelType = entityMap.ModelType;

            int count = Math.Min(
              storeType.Properties.Count,
              modelType.Properties.Count);

            for (int i = 0; i < count; i++)
            {
                // assume properties are in same order
                var store = storeType.Properties[i];
                var model = modelType.Properties[i];

                var map = new PropertyMap
                {
                    PropertyName = model.Name,
                    ColumnName = QuoteIdentifier(store.Name)
                };
                entityMap.PropertyMaps.Add(map);
            }
        }

        private static void SetTableName(EntityMap entityMap)
        {
            var builder = new StringBuilder(50);

            EntitySet storeSet = entityMap.StoreSet;
            dynamic storeSetProxy = new DynamicProxy(storeSet);

            string schema = storeSetProxy.Schema;
            string table = storeSetProxy.Table;

            if (!string.IsNullOrEmpty(schema))
            {
                builder.Append(QuoteIdentifier(schema));
                builder.Append(".");
            }
            else
            {
                builder.Append(QuoteIdentifier(storeSet.EntityContainer.Name));
                builder.Append(".");
            }

            if (!string.IsNullOrEmpty(table))
                builder.Append(QuoteIdentifier(table));
            else
                builder.Append(QuoteIdentifier(storeSet.Name));

            entityMap.TableName = builder.ToString();
        }

        private static string QuoteIdentifier(string name)
        {
            return ("[" + name.Replace("]", "]]") + "]");
        }
    }

    [DebuggerDisplay("Table: {TableName}")]
    public class EntityMap
    {
        public EntityMap(Type entityType)
        {
            _entityType = entityType;
            _keyMaps = new List<PropertyMap>();
            _propertyMaps = new List<PropertyMap>();
        }

        public EntitySet ModelSet { get; set; }
        public EntitySet StoreSet { get; set; }

        public EntityType ModelType { get; set; }
        public EntityType StoreType { get; set; }

        private readonly Type _entityType;
        public Type EntityType
        {
            get { return _entityType; }
        }
        public string TableName { get; set; }

        private readonly List<PropertyMap> _propertyMaps;
        public List<PropertyMap> PropertyMaps
        {
            get { return _propertyMaps; }
        }

        private readonly List<PropertyMap> _keyMaps;
        public List<PropertyMap> KeyMaps
        {
            get { return _keyMaps; }
        }

    }

    [DebuggerDisplay("Property: {PropertyName}, Column: {ColumnName}")]
    public class PropertyMap
    {
        public string PropertyName { get; set; }
        public string ColumnName { get; set; }
    }
}
