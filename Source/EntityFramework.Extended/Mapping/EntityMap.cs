using System;
using System.Collections.Generic;
using System.Data.Metadata.Edm;
using System.Diagnostics;
using EntityFramework.Extensions;

namespace EntityFramework.Mapping
{
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
}